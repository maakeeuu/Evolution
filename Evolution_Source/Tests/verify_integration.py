"""Static integration checks only. Never starts or validates Unity."""
from pathlib import Path
import hashlib
import json
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Evolution_Source'
ASSETS = ROOT / 'Assets'
MIRROR = ASSETS / 'Evolution' / 'EVCP1'

class IntegrationTests(unittest.TestCase):
    def test_runtime_mirror_has_exact_sources_and_no_tools(self):
        expected = {p.relative_to(SOURCE) for folder in ('Code', 'Art')
                    for p in (SOURCE / folder).rglob('*') if p.is_file() and p.suffix != '.meta'}
        actual = {p.relative_to(MIRROR) for p in MIRROR.rglob('*') if p.is_file() and p.suffix != '.meta'}
        self.assertEqual(len(expected), 13)
        self.assertEqual(actual, expected)
        for relative in expected:
            self.assertEqual((SOURCE / relative).read_bytes(), (MIRROR / relative).read_bytes(), str(relative))
        self.assertFalse(any(p.suffix in ('.py', '.csproj') for p in ASSETS.rglob('*')))

    def test_unique_unity_assemblies_and_core_separation(self):
        definitions = [json.loads(p.read_text()) for p in ASSETS.rglob('*.asmdef')]
        names = [d['name'] for d in definitions]
        self.assertEqual(len(names), len(set(names)))
        self.assertEqual(set(names), {'Evolution.Source.Core', 'Evolution.Source.Unity', 'Evolution.Articulated.Core', 'Evolution.Articulated.Unity','Evolution.Articulated.Editor'})
        core = next(d for d in definitions if d['name'] == 'Evolution.Source.Core')
        runtime = next(d for d in definitions if d['name'] == 'Evolution.Source.Unity')
        self.assertTrue(core['noEngineReferences'])
        self.assertEqual(runtime['references'], ['Evolution.Source.Core'])
        for p in (MIRROR / 'Code' / 'Core').glob('*.cs'):
            self.assertNotIn('using UnityEngine', p.read_text())

    def test_shader_has_one_resources_key_and_expected_property(self):
        shaders = [p for p in ASSETS.rglob('EvolutionVector2D.shader') if 'Resources' in p.parts]
        self.assertEqual(len(shaders), 1)
        self.assertIn('Shader "Evolution/Vector2D"', shaders[0].read_text())
        self.assertIn('_Color', shaders[0].read_text())
        for name in ('CreatureBody2D', 'VectorObstacle2D'):
            self.assertIn('Resources.Load<Shader>("EvolutionVector2D")',
                          (MIRROR / 'Code' / 'Unity' / (name+'.cs')).read_text())

    def test_legacy_coexistence_and_attribute_fix(self):
        legacy = (ASSETS / 'Evolution/Scripts/Creature/ProceduralCreature2D.cs').read_text()
        self.assertIn('namespace Evolution.Creatures', legacy)
        self.assertIn('[RequireComponent(typeof(LineRenderer))]', legacy)
        editor = (ASSETS / 'Editor/EvolutionDemoSceneBuilder.cs').read_bytes()
        blob = b'blob ' + str(len(editor)).encode() + b'\0' + editor
        self.assertEqual(hashlib.sha1(blob).hexdigest(), '0fcc3bd84eb66bfaabb8526b87e1bea606c70202')
        for p in (MIRROR / 'Code' / 'Unity').glob('*.cs'):
            self.assertIn('namespace Evolution.Source.Unity', p.read_text())
            self.assertNotIn('namespace Evolution.Creatures', p.read_text())

    def test_engine_and_only_required_builtin_dependency(self):
        version = (ROOT / 'ProjectSettings/ProjectVersion.txt').read_text().strip()
        self.assertEqual(version, 'm_EditorVersion: 6000.6.10f1')
        manifest = json.loads((ROOT / 'Packages/manifest.json').read_text())
        self.assertEqual(manifest['dependencies'], {'com.unity.modules.physics2d': '1.0.0','com.unity.modules.imgui': '1.0.0'})

    def test_existing_meta_guids_are_well_formed_and_unique(self):
        guids = []
        for path in ASSETS.rglob('*.meta'):
            match = re.search(r'^guid: ([0-9a-f]{32})$', path.read_text(), re.M)
            self.assertIsNotNone(match, str(path))
            guids.append(match.group(1))
        self.assertEqual(len(guids), len(set(guids)))
        # A fresh clone has no .meta yet; absence is NOT proof of Unity reference validation.

if __name__ == '__main__':
    unittest.main()
