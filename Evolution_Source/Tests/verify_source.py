"""Structural smoke checks only. These deliberately do not claim C# compilation."""
from pathlib import Path
import json
import re
import unittest

ROOT=Path(__file__).resolve().parents[1]

def source(name):
    return (ROOT/'Code/Unity'/name).read_text()

def body(text, method):
    start=text.index('private void '+method+'(')
    start=text.index('{',start)
    level=1
    end=start+1
    while level:
        level+=(text[end]=='{')-(text[end]=='}')
        end+=1
    return text[start+1:end-1]

class SourceTests(unittest.TestCase):
    def test_no_four_type_requirecomponent(self):
        for path in (ROOT/'Code').rglob('*.cs'):
            for arguments in re.findall(r'\[RequireComponent\(([^\]]+)\)\]',path.read_text()):
                self.assertLessEqual(arguments.count('typeof('),3,str(path))

    def test_same_float_contour_and_fixed_step(self):
        text=source('CreatureBody2D.cs')
        self.assertIn('new Vector3(physicsContour[i].x, physicsContour[i].y, 0f)',text)
        self.assertIn('polygon.SetPath(0, physicsContour)',text)
        self.assertIn('Time.fixedDeltaTime',body(text,'FixedUpdate'))
        self.assertNotIn('private void Update(',text)
        self.assertNotIn('[ExecuteAlways]',text)
        self.assertNotIn('LineRenderer',text)
        self.assertIn('RadialGeometry.FillFloatCompatible(shape, contour)',text)
        self.assertIn('if (!changed)',text)
        self.assertIn('if (physicsBody != null) physicsBody.WakeUp();',text)
        publish=body(text,'Publish') if 'private void Publish(' in text else ''
        self.assertLess(publish.index('if (!changed)'),publish.index('polygon.SetPath'))
        self.assertLess(publish.index('ContourValidator.IsValidFan'),publish.index('mesh.vertices'))

    def test_validation_does_not_rebuild_or_reset(self):
        validation=body(source('CreatureBody2D.cs'),'OnValidate')
        self.assertIn('NormalizeSettings();',validation)
        self.assertNotRegex(validation,r'\b(new|Publish|Destroy|current\s*=|target\s*=)\b')

    def test_assembly_and_shader_contract(self):
        core=json.loads((ROOT/'Code/Core/Evolution.Source.Core.asmdef').read_text())
        adapter=json.loads((ROOT/'Code/Unity/Evolution.Source.Unity.asmdef').read_text())
        self.assertTrue(core['noEngineReferences'])
        self.assertIn(core['name'],adapter['references'])
        shader=(ROOT/'Art/Resources/EvolutionVector2D.shader').read_text()
        self.assertIn('Cull Off',shader)
        self.assertIn('_Color',shader)
        self.assertIn('Resources.Load<Shader>("EvolutionVector2D")',source('CreatureBody2D.cs'))

    def test_optional_unity_suite_is_separate(self):
        asm=json.loads((ROOT/'Tests/Unity/PlayMode/Evolution.Source.PlayModeTests.asmdef').read_text())
        self.assertIn('UNITY_INCLUDE_TESTS',asm['defineConstraints'])
        self.assertIn('Evolution.Source.Unity',asm['references'])
        tests=(ROOT/'Tests/Unity/PlayMode/CreatureIntegrationTests.cs').read_text()
        self.assertEqual(tests.count('[UnityTest]'),3)
        project=(ROOT/'Tests/Standalone/Evolution.Core.Tests.csproj').read_text()
        self.assertNotIn('Tests/Unity',project)
        self.assertNotIn('UnityEngine',project)

    def test_initialization_failure_and_resource_ownership(self):
        for name in ('CreatureBody2D.cs','VectorObstacle2D.cs'):
            text=source(name)
            self.assertIn('catch (Exception error)',text)
            self.assertIn('ReleaseResources();',text)
            self.assertIn('if (failed) enabled = false;',text)
            self.assertIn('polygon.enabled = false;',text)
            self.assertIn('previousProperties',text)
            self.assertNotIn('Destroy(vectorMaterial)',text)
            self.assertIn('meshRenderer.sharedMaterial = previousMaterial;',text)
        self.assertIn('!shape.IsInitialized',source('CreatureMotor2D.cs'))
        self.assertIn('AbortInitialization()',source('EvolutionPrototype2D.cs'))

    def test_delimiters_smoke_check(self):
        # Strip comments and strings before simple delimiter checking. Not a parser.
        token=re.compile(r'//[^\n]*|/\*.*?\*/|@"(?:[^"]|"")*"|"(?:\\.|[^"\\])*"',re.S)
        for path in (ROOT/'Code').rglob('*.cs'):
            text=token.sub('',path.read_text())
            stack=[]
            for char in text:
                if char in '([{': stack.append(char)
                elif char in ')]}':
                    self.assertTrue(stack,str(path))
                    self.assertEqual(stack.pop(),{')':'(',']':'[','}':'{'}[char],str(path))
            self.assertEqual(stack,[],str(path))

if __name__=='__main__': unittest.main(verbosity=2)
