using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CSToGDScript
{

    public static class ExtStringBuilder
    {
        public static StringBuilder AppendTabs(this StringBuilder sb, int depth)
        {
            var tabs = string.Concat(System.Linq.Enumerable.Repeat("\t", depth));
            sb.Append(tabs);
            return sb;
        }
    }
    internal class CSToGDScript
    {
        public CSToGDScript()
        {

        }



        public static string HandleType(TypeSyntax type)
        {
            if (type.GetType() == typeof(IdentifierNameSyntax))
                return (type as IdentifierNameSyntax).Identifier.Text;
            if (type.GetType() == typeof(PredefinedTypeSyntax))
                return (type as PredefinedTypeSyntax).Keyword.Text;
            return "";
        }

        public static void HandleExpression(ExpressionSyntax expr, StringBuilder sb)
        {
            if (expr.GetType() == typeof(InvocationExpressionSyntax))
            {
                var invocExpr = expr as InvocationExpressionSyntax;
                HandleExpression(invocExpr.Expression, sb);
                sb.Append("(");
                foreach (var arg in invocExpr.ArgumentList.Arguments)
                {
                    HandleExpression(arg.Expression, sb);
                    if (arg != invocExpr.ArgumentList.Arguments.Last())
                        sb.Append(", ");
                }
                sb.Append(")");
            }
            if (expr.GetType() == typeof(MemberAccessExpressionSyntax))
            {
                var accessExpr = expr as MemberAccessExpressionSyntax;
                HandleExpression(accessExpr.Expression, sb);
                sb.Append(accessExpr.Name.Identifier.Text);
                //accessExpr
                //accessExpr.Expression
            }
            if (expr.GetType() == typeof(ThisExpressionSyntax))
            {
                var thisExpr = expr as ThisExpressionSyntax;
                sb.Append("self.");
            }
        }

        public static void HandlePropertyDeclarationSyntax(PropertyDeclarationSyntax decl, StringBuilder sb, int depth)
        {
            sb.Append($@"@export var {decl.Identifier.Text}: {HandleType(decl.Type)}");
            if (decl.Initializer != null)
            {
                sb.Append(" = ");
                HandleExpression(decl.Initializer.Value, sb);
            }
            sb.AppendLine();
        }
        public static void HandleStatementSyntax(StatementSyntax stmt, StringBuilder sb, int depth)
        {
            if (stmt == null)
                sb.AppendTabs(depth).AppendLine("pass");
            if (stmt.GetType() == typeof(BlockSyntax))
            {
                var block = stmt as BlockSyntax;
                foreach (var subStmt in block.Statements)
                {
                    HandleStatementSyntax(subStmt, sb, depth + 1);
                }
            }
            if (stmt.GetType() == typeof(ExpressionStatementSyntax))
            {
                var exprStmt = stmt as ExpressionStatementSyntax;
                sb.AppendTabs(depth);
                HandleExpression(exprStmt.Expression, sb);
                sb.AppendLine();
            }
        }


        public static void HandleMethodDeclarationSyntax(MethodDeclarationSyntax decl, StringBuilder sb, int depth)
        {
            HashSet<string> gdFunctions = new HashSet<string>
            {
                "_Ready",
                "_Process",
                "_PhysicsProcess"
            };
            string funcName = decl.Identifier.Text;
            if (gdFunctions.Contains(funcName))
                funcName = funcName.ToLower();
            sb.AppendTabs(depth).Append($@"func {funcName}(");
            foreach (var param in decl.ParameterList.Parameters)
            {
                sb.Append(param.Identifier.Text);
                if (param != decl.ParameterList.Parameters.Last())
                    sb.Append(", ");
            }

            sb.AppendLine("):");
            HandleStatementSyntax(decl.Body, sb, depth);
        }
        public static void HandleMemberDeclarationSyntax(MemberDeclarationSyntax decl, StringBuilder sb, int depth)
        {

            if (decl.GetType() == typeof(PropertyDeclarationSyntax))
                HandlePropertyDeclarationSyntax(decl as PropertyDeclarationSyntax, sb, depth);
            if (decl.GetType() == typeof(MethodDeclarationSyntax))
                HandleMethodDeclarationSyntax(decl as MethodDeclarationSyntax, sb, depth);
            //Console.WriteLine(decl);

        }

        public static void HandleClassDecl(ClassDeclarationSyntax class_, StringBuilder sb, int depth)
        {
            // class decl +extends baseclass https://docs.godotengine.org/fr/stable/tutorials/scripting/gdscript/gdscript_basics.html

            var firstBase = class_.BaseList.Types.First();
            if (firstBase != null)
                sb.AppendLine($@"extends {HandleType(firstBase.Type)}");

            sb.AppendLine($@"class_name {class_.Identifier.Text}");
            foreach (var decl in class_.Members)
            {
                HandleMemberDeclarationSyntax(decl, sb, depth);
            }
        }


        public static void HandleDecls(SyntaxList<MemberDeclarationSyntax> decls, StringBuilder sb, int depth)
        {
            foreach (var member in decls)
            {
                if (member.GetType() == typeof(ClassDeclarationSyntax))
                    HandleClassDecl(member as ClassDeclarationSyntax, sb, depth);
            }

        }

        public static string TranspileFile(string path)
        {
            var programText = File.ReadAllText(path);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            StringBuilder sb = new StringBuilder();
            HandleDecls(root.Members, sb, 0);

            return sb.ToString();
        }


        public static void BrowseFiles(string path)
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (System.IO.Path.GetExtension(file) == ".cs")
                {
                    var gdScriptPath = file.Replace(".cs", ".gdscript");
                    Console.WriteLine(gdScriptPath);
                    var gdScriptCode = TranspileFile(file);
                    File.WriteAllText(gdScriptPath, gdScriptCode);
                    Console.WriteLine(gdScriptCode);
                }
            }
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                BrowseFiles(dir);
            }
        }

        public static void HandleProgram()
        {
            string folderPath = "C:\\Users\\s.maire\\Desktop\\ldjam-53-green";

            BrowseFiles(folderPath);
        }
    }
}
