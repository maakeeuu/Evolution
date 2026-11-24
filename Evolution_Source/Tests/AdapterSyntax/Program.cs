using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1) throw new ArgumentException("Pass the Evolution_Source directory.");
        string root = Path.GetFullPath(args[0]);
        string[] directories = { Path.Combine(root,"Code","Unity"), Path.Combine(root,"Tests","Unity"), Path.Combine(Directory.GetParent(root).FullName,"Assets") };
        string[] callbacks = { "Awake", "Start", "Update", "FixedUpdate", "OnEnable", "OnDisable", "OnDestroy", "OnValidate" };
        int files=0, behaviours=0, contracts=0;
        foreach (string directory in directories)
        foreach (string path in Directory.GetFiles(directory,"*.cs",SearchOption.AllDirectories).OrderBy(p=>p,StringComparer.Ordinal))
        {
            files++;
            SyntaxTree tree=CSharpSyntaxTree.ParseText(File.ReadAllText(path),new CSharpParseOptions(LanguageVersion.CSharp9).WithPreprocessorSymbols("UNITY_EDITOR","ENABLE_LEGACY_INPUT_MANAGER","UNITY_INCLUDE_TESTS"),path);
            var errors=tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray();
            if(errors.Length>0) throw new Exception(string.Join(Environment.NewLine,errors.Select(d=>d.ToString())));
            foreach(ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if(type.BaseList==null || !type.BaseList.Types.Any(b=>b.Type.ToString()=="MonoBehaviour")) continue;
                behaviours++;
                if(type.Identifier.ValueText!=Path.GetFileNameWithoutExtension(path))
                    throw new Exception("MonoBehaviour/file name mismatch: "+path);
                foreach(MethodDeclarationSyntax method in type.Members.OfType<MethodDeclarationSyntax>())
                {
                    if(!callbacks.Contains(method.Identifier.ValueText)) continue;
                    contracts++;
                    if(method.ParameterList.Parameters.Count!=0 || method.ReturnType.ToString()!="void"
                        || method.Modifiers.Any(SyntaxKind.StaticKeyword))
                        throw new Exception("Invalid lifecycle signature: "+path+" / "+method.Identifier.ValueText);
                }
                foreach(AttributeSyntax attribute in type.AttributeLists.SelectMany(a=>a.Attributes))
                {
                    if(attribute.Name.ToString()=="RequireComponent" && attribute.ArgumentList!=null
                        && attribute.ArgumentList.Arguments.Count>3)
                        throw new Exception("Invalid RequireComponent arity: "+path);
                }
            }
        }
        if(files==0 || behaviours==0) throw new Exception("No adapter sources were parsed.");
        Console.WriteLine("PASS: "+files+" actual source files parsed as C# 9; "+behaviours+
            " MonoBehaviours; "+contracts+" lifecycle signatures. Syntax/contracts only; NO Unity API compilation or execution.");
    }
}
