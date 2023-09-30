using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Collections;

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
        // https://docs.godotengine.org/en/latest/tutorials/scripting/c_sharp/c_sharp_differences.html#random-functions
        static string HandleSyntaxToken(SyntaxToken token)
        {
            Dictionary<string, string> godotFunctions = new Dictionary<string, string>()
            {
                { "Transform", "self.Transform" }, // Meh
                { "GetVector", "get_vector" },
                { "GetSetting", "get_setting" },
                { "SetSetting", "set_setting" },
                { "HasSetting", "has_setting" },
                { "GetNode", "get_node" },
                { "GetViewport", "get_viewport" },
                { "GetCamera2D", "get_camera_2d" },
                { "GlobalPosition", "global_position" },
                { "GlobalRotation", "global_rotation" },
                { "QueueFree", "queue_free" },
                { "_Ready", "_ready" },
                { "_Process", "_process" },
                { "_PhysicsProcess", "_physics_process" },
                { "Text", "text" }, // Not idea as it can trigger use names too but would need to do a bit of semantic along the way to handle that properly
                { "Visible", "visible" },
                { "GetTree", "get_tree" },
                { "GetChildren", "get_children"},
                { "AddChild", "add_child" },
                { "Instantiate", "instantiate" },
                { "RandRange", "randf_range" },
                { "Print", "print" },
                { "Prints", "prints" },
                { "Printt", "printt" },
                { "PrintRaw", "printraw" },
                { "PrintErr", "printerr" },
                { "PrintRich", "print_rich" },
                { "Load", "preload" },
                { "RemoveChild", "remove_child" },
                { "Color", "Color" },
                { "Position", "position" },
                { "X", "x" },
                { "Y", "y" },
                { "Z", "z" },
                { "W", "w" },
                { "Length","size()" },
                { "Count","size()" },
                { "Material","material" },
                { "SetShaderParameter","set_shader_parameter" },
                { "GetActionStrength", "get_action_strength" },
                { "Name", "name" },
                { "Normalized", "normalized" },
                { "Atan2", "atan2" },
                { "DistanceTo", "distance_to" },
                { "IsActionPressed", "is_action_pressed" },
                { "IsActionReleased", "is_action_released" },
                { "IsActionJustPressed", "is_action_just_pressed" },
                { "IsActionJustReleased", "is_action_just_released" },
                { "GetAxis", "get_axis" },
                { "ChangeSceneToFile", "change_scene_to_file" },
                { "ToSignal", "" },
                { "CreateTimer", "create_timer" },
                { "Hide", "hide" },
                { "Show", "show" },
                { "Start", "start" },
                { "WaitTime", "wait_time" },

                { "Min", "min" },
                { "Max", "max" },
                { "Clamp", "clamp" },
                { "Lerp", "lerp" },
                { "Slerp", "slerp" },
                { "Play", "play"},
                { "MoveAndSlide", "move_and_slide"},
                { "Velocity", "velocity"},

                { "LimitLength", "limit_length"},
                { "Rotated", "rotated"},
                { "Rotate", "rotate"},
                { "DegToRad", "deg_to_rad"},
                { "MoveToward", "move_toward" },
                { "Rotation", "rotation" },
                { "Zero", "ZERO" },
                { "One", "ONE" },
                { "Up", "UP" },
                { "Down", "DOWN" },
                { "Left", "LEFT" },
                { "Right", "RIGHT" },
                {"Texture", "texture" },

                { "Scale", "scale"},
                { "Abs", "abs" },
                { "Cos", "cos" },
                { "Sin", "sin" },
                { "Cosh", "cosh" },
                { "Sinh", "sinh" },
                { "Tan", "tan" },
                { "Tanh", "tanh" },
                { "Exp", "exp" },
                { "Log", "log" },
                { "Pow", "pow" },
                { "Sqrt", "sqrt" },
                { "Floor", "floor" },
                { "Ceil", "ceil" },
                { "Round", "round" },

                { "CsgMesh3D", "CSGMesh3D" },
            };
            string res = token.Text;
            string output = "";
            if (godotFunctions.TryGetValue(token.Text, out output))
                res = output;
            return res;
        }

        public static string HandleType(TypeSyntax type)
        {
            if (type == null) return "";
            if (type.GetType() == typeof(IdentifierNameSyntax))
                return HandleSyntaxToken((type as IdentifierNameSyntax).Identifier);
            else if (type.GetType() == typeof(PredefinedTypeSyntax))
            {
                var predefType = (type as PredefinedTypeSyntax).Keyword.Text;
                if (predefType == "string")
                    predefType = "String";
                else if (predefType == "double")
                    predefType = "float";
                else if (predefType == "long")
                    predefType = "int";

                return predefType;
            }
            else if (type.GetType() == typeof(ArrayTypeSyntax))
            {
                var arrayType = type as ArrayTypeSyntax;
                if (arrayType.ElementType.GetType() == typeof(PredefinedTypeSyntax))
                {
                    var identArrayType = arrayType.ElementType as PredefinedTypeSyntax;
                    if (identArrayType.Keyword.Text.ToLower() == "string")
                    {
                        return "PackedStringArray";
                    }
                    else if (identArrayType.Keyword.Text.ToLower() == "int")
                    {
                        return "PackedInt32Array";
                    }
                    else if (identArrayType.Keyword.Text.ToLower() == "float")
                    {
                        return "PackedFloatArray";
                    }
                }
                return "Array";
            }
            else if (type.GetType() == typeof(GenericNameSyntax))
            {
                var genericType = type as GenericNameSyntax;
                if (genericType.Identifier.Text == "List")
                    return "Array";
                return genericType.Identifier.Text;
            }
            else if (type.GetType() == typeof(QualifiedNameSyntax))
            {
                var qualifiedType = type as QualifiedNameSyntax;
                return qualifiedType.ToString();

            }
            else
            {

            }
            return "";
        }

        public static void HandleArgumentList(BaseArgumentListSyntax list, StringBuilder sb)
        {
            foreach (var arg in list.Arguments)
            {
                HandleExpression(arg.Expression, sb);
                if (arg != list.Arguments.Last())
                    sb.Append(", ");
            }
        }
        public static async void HandleExpression(ExpressionSyntax expr, StringBuilder sb, TypeSyntax inferedType = null)
        {
            if (expr == null) return;

            Dictionary<string, string> mapUnaryOperators = new Dictionary<string, string>()
                {
                    {"++", " += 1" },
                    {"--", " -= 1" },
                };
            
            if (expr.GetType() == typeof(InvocationExpressionSyntax))
            {
                var invocExpr = expr as InvocationExpressionSyntax;
                HandleExpression(invocExpr.Expression, sb);

                bool handleToString = expr.ToString().Contains("ToString");
                if (!handleToString)
                    sb.Append("(");
                HandleArgumentList(invocExpr.ArgumentList, sb);
                if (!handleToString)
                    sb.Append(")");
            }
            else if (expr.GetType() == typeof(MemberAccessExpressionSyntax))
            {
                
                var accessExpr = expr as MemberAccessExpressionSyntax;
                
                var accessAsIdentifierName = accessExpr.Expression as IdentifierNameSyntax;
                if (accessExpr.Name.Identifier.Text == "ToString")
                {
                    sb.Append("str(");
                    HandleExpression(accessExpr.Expression, sb);
                    sb.Append(")");
                }
                else if (accessAsIdentifierName != null &&
                    (accessAsIdentifierName.Identifier.Text == "GD" ||
                    accessAsIdentifierName.Identifier.Text == "Mathf" ||
                    accessAsIdentifierName.Identifier.Text == "MathF" ||
                    accessAsIdentifierName.Identifier.Text == "Math" ||
                    accessAsIdentifierName.Identifier.Text == "ResourceLoader"))
                {
                    sb.Append(HandleSyntaxToken(accessExpr.Name.Identifier));
                }
                else
                {
                    HandleExpression(accessExpr.Expression, sb);
                    sb.Append(".");
                    sb.Append(HandleSyntaxToken(accessExpr.Name.Identifier));
                }
                //accessExpr
                //accessExpr.Expression
            }
            else if (expr.GetType() == typeof(ThisExpressionSyntax))
            {
                var thisExpr = expr as ThisExpressionSyntax;
                sb.Append("self");
            }
            else if (expr.GetType() == typeof(BaseExpressionSyntax))
            {
                var baseExpr = expr as BaseExpressionSyntax;
                sb.Append("super");                
            }
            else if (expr.GetType() == typeof(AssignmentExpressionSyntax))
            {
                var assignExpr = expr as AssignmentExpressionSyntax;

                HandleExpression(assignExpr.Left, sb);
                sb.Append(" ").Append(assignExpr.OperatorToken).Append(" ");
                HandleExpression(assignExpr.Right, sb);
            }
            else if (expr.GetType() == typeof(IdentifierNameSyntax))
            {
                var identifierExpr = expr as IdentifierNameSyntax;
                sb.Append(HandleSyntaxToken(identifierExpr.Identifier));
            }
            else if (expr.GetType() == typeof(LiteralExpressionSyntax))
            {
                var literalExpr = expr as LiteralExpressionSyntax;

                var regexIsFloat = new Regex(@"^[0-9]*(?:\.[0-9]*)?f$");
                var regexIsDouble = new Regex(@"^[0-9]*(?:\.[0-9]*)?d$");
                if (regexIsFloat.IsMatch(literalExpr.Token.Text)) // example: 2.0f
                    sb.Append(literalExpr.Token.Text.Replace("f", ""));
                else if (regexIsDouble.IsMatch(literalExpr.Token.Text)) // example: 2d
                    sb.Append(literalExpr.Token.Text.Replace("d", ""));
                else
                    sb.Append(literalExpr.Token.Text);

            }
            else if (expr.GetType() == typeof(GenericNameSyntax))
            {
                var genericNameExpr = expr as GenericNameSyntax; // GetNode<Type> has no equivalent in python
                sb.Append(HandleSyntaxToken(genericNameExpr.Identifier));
            }
            else if (expr.GetType() == typeof(ArrayCreationExpressionSyntax))
            {
                var arrayCreateExpr = expr as ArrayCreationExpressionSyntax;
                sb.Append("[");
                HandleExpression(arrayCreateExpr.Initializer, sb);
                sb.Append("]");
            }
            else if (expr.GetType() == typeof(BinaryExpressionSyntax))
            {
                var binaryExpr = expr as BinaryExpressionSyntax;
                HandleExpression(binaryExpr.Left, sb);
                sb.Append(" ").Append(HandleSyntaxToken(binaryExpr.OperatorToken)).Append(" ");
                HandleExpression(binaryExpr.Right, sb);
            }
            else if (expr.GetType() == typeof(PrefixUnaryExpressionSyntax))
            {
                var unaryExpr = expr as PrefixUnaryExpressionSyntax;
                sb.Append(unaryExpr.OperatorToken);
                HandleExpression(unaryExpr.Operand, sb);
            }
            else if (expr.GetType() == typeof(InitializerExpressionSyntax))
            {
                var initializerExpr = expr as InitializerExpressionSyntax;
                foreach (var val in initializerExpr.Expressions)
                {
                    HandleExpression(val, sb);
                    if (val != initializerExpr.Expressions.Last())
                        sb.Append(", ");
                }
            }
            else if (expr.GetType() == typeof(ObjectCreationExpressionSyntax))
            {
                var objCreateExpr = expr as ObjectCreationExpressionSyntax;
                //sb.Append("new ");
                sb.Append(HandleType(objCreateExpr.Type));
                sb.Append("(");
                HandleArgumentList(objCreateExpr.ArgumentList, sb);
                sb.Append(")");
            }
            else if (expr.GetType() == typeof(CastExpressionSyntax))
            {
                var castExpr = expr as CastExpressionSyntax;
                sb.Append("(");
                HandleExpression(castExpr.Expression, sb);
                sb.Append(" as ");
                sb.Append(HandleType(castExpr.Type));
                sb.Append(")");
            }
            else if (expr.GetType() == typeof(ParenthesizedExpressionSyntax))
            {
                var parenthExpr = expr as ParenthesizedExpressionSyntax;
                sb.Append("(");
                HandleExpression(parenthExpr.Expression, sb);
                sb.Append(")");
            }
            else if (expr.GetType() == typeof(ImplicitObjectCreationExpressionSyntax))
            {
                var implicitExpr = expr as ImplicitObjectCreationExpressionSyntax;
                //sb.Append("new ");
                sb.Append(HandleType(inferedType));
                sb.Append("(");
                HandleArgumentList(implicitExpr.ArgumentList, sb);
                sb.Append(")");
            }
            else if (expr.GetType() == typeof(ElementAccessExpressionSyntax))
            {
                var elementAccessExpr = expr as ElementAccessExpressionSyntax;
                HandleExpression(elementAccessExpr.Expression, sb);
                sb.Append("[");
                HandleArgumentList(elementAccessExpr.ArgumentList, sb); // Will generated invalid gdscript if [a,b]
                sb.Append("]");
            }
            else if (expr.GetType() == typeof(PostfixUnaryExpressionSyntax))
            {
                var postFixExpr = expr as PostfixUnaryExpressionSyntax;
                Dictionary<string, string> mapOperators = new Dictionary<string, string>()
                {
                    {"++", " += 1" },
                    {"--", " -= 1" },
                };
                HandleExpression(postFixExpr.Operand, sb);
                string newOp = "";
                if (mapUnaryOperators.TryGetValue(postFixExpr.OperatorToken.Text, out newOp))
                    sb.Append(newOp);
                else
                    sb.Append(postFixExpr.OperatorToken);
            }
            else if (expr.GetType() == typeof(PrefixUnaryExpressionSyntax))
            {
                var postFixExpr = expr as PostfixUnaryExpressionSyntax;

                HandleExpression(postFixExpr.Operand, sb);
                string newOp = "";
                if (mapUnaryOperators.TryGetValue(postFixExpr.OperatorToken.Text, out newOp))
                    sb.Append(newOp);
                else
                    sb.Append(postFixExpr.OperatorToken);
            }
            else if (expr.GetType() == typeof(AwaitExpressionSyntax))
            {
                var awaitExpr = expr as AwaitExpressionSyntax;
                sb.Append("await ");
                // ToSignal(argA, argB) to await argA.argB
                var awaitInvoc = awaitExpr.Expression as InvocationExpressionSyntax;
                if (awaitInvoc != null)
                {
                    HandleExpression(awaitInvoc.ArgumentList.Arguments.First().Expression, sb);
                    sb.Append(".");
                    var access = awaitInvoc.ArgumentList.Arguments.Last().ToString().Replace("\"", "");
                    sb.Append(access);
                }
            }
            else
            {

            }
        }
        public static void HandleInitialize(EqualsValueClauseSyntax equals, StringBuilder sb, TypeSyntax inferedType)
        {
            sb.Append(" = ");
            HandleExpression(equals.Value, sb, inferedType);
        }
        public static void HandlePropertyDeclarationSyntax(PropertyDeclarationSyntax decl, StringBuilder sb, int depth)
        {
            sb.Append($@"var {decl.Identifier.Text}");
            if (HandleType(decl.Type) != "var")
                sb.Append($": {HandleType(decl.Type)}");
            if (decl.Initializer != null)
                HandleInitialize(decl.Initializer, sb, decl.Type);
            sb.AppendLine();
        }
        public static void HandleStatementSyntax(StatementSyntax stmt, StringBuilder sb, int depth)
        {
            if (stmt == null)
            {
                sb.AppendTabs(depth).AppendLine("pass");
                return;
            }
            if (stmt.GetType() == typeof(BlockSyntax))
            {
                var block = stmt as BlockSyntax;
                if (block.Statements.Count == 0)
                    sb.AppendTabs(depth).AppendLine("pass");
                foreach (var subStmt in block.Statements)
                {
                    HandleStatementSyntax(subStmt, sb, depth);
                }
            }
            else if (stmt.GetType() == typeof(ExpressionStatementSyntax))
            {
                var exprStmt = stmt as ExpressionStatementSyntax;
                sb.AppendTabs(depth);
                HandleExpression(exprStmt.Expression, sb);
                sb.AppendLine();
            }
            else if (stmt.GetType() == typeof(ReturnStatementSyntax))
            {
                var retStmt = stmt as ReturnStatementSyntax;
                sb.AppendTabs(depth).Append("return ");
                HandleExpression(retStmt.Expression, sb);
                sb.AppendLine();
            }
            else if (stmt.GetType() == typeof(IfStatementSyntax))
            {
                var ifStmt = stmt as IfStatementSyntax;
                sb.AppendTabs(depth);
                sb.Append("if ");
                HandleExpression(ifStmt.Condition, sb);
                sb.AppendLine(":");
                HandleStatementSyntax(ifStmt.Statement, sb, depth + 1);
                if (ifStmt.Else != null)
                {
                    var elseClause = ifStmt.Else;
                    sb.AppendTabs(depth);
                    sb.AppendLine("else:");
                    HandleStatementSyntax(elseClause.Statement, sb, depth + 1);
                }
            }
            else if (stmt.GetType() == typeof(LocalDeclarationStatementSyntax))
            {
                var localDeclStmt = stmt as LocalDeclarationStatementSyntax;
                HandleVariableDeclarationSyntax(localDeclStmt.Declaration, sb, depth);
            }
            else if (stmt.GetType() == typeof(SwitchStatementSyntax))
            {
                var switchStmt = stmt as SwitchStatementSyntax;
                sb.AppendTabs(depth);
                sb.Append("match ");
                HandleExpression(switchStmt.Expression, sb);
                sb.AppendLine(":");
                foreach (var clause in switchStmt.Sections)
                {
                    sb.AppendTabs(depth + 1);
                    foreach (var label in clause.Labels)
                    {
                        if (label.GetType() == typeof(CaseSwitchLabelSyntax))
                        {
                            var caseSwitch = label as CaseSwitchLabelSyntax;
                            HandleExpression(caseSwitch.Value, sb);
                            sb.AppendLine(":");
                        }
                    }
                    foreach (var stmtSwitch in clause.Statements)
                    {
                        HandleStatementSyntax(stmtSwitch, sb, depth + 2);
                    }
                }
            }
            else if (stmt.GetType() == typeof(BreakStatementSyntax))
            {
                var breakStmt = stmt as BreakStatementSyntax;
                if (breakStmt.Parent.GetType() != typeof(SwitchStatementSyntax)) // No breaks in python switches
                    sb.AppendTabs(depth).AppendLine("break");
            }
            else if (stmt.GetType() == typeof(ContinueStatementSyntax))
            {
                sb.AppendTabs(depth).AppendLine("continue");
            }
            else if (stmt.GetType() == typeof(ForStatementSyntax))
            {
                var forStmt = stmt as ForStatementSyntax;
                HandleVariableDeclarationSyntax(forStmt.Declaration, sb, depth);
                sb.AppendTabs(depth);
                sb.Append("while ");
                HandleExpression(forStmt.Condition, sb);
                sb.AppendLine(":");
                HandleStatementSyntax(forStmt.Statement, sb, depth + 1);
                foreach (var incrementor in forStmt.Incrementors)
                {
                    sb.AppendTabs(depth + 1);
                    HandleExpression(incrementor, sb);
                    sb.AppendLine();
                }
            }
            else if (stmt.GetType() == typeof(WhileStatementSyntax))
            {
                var whileStmt = stmt as WhileStatementSyntax;
                sb.AppendTabs(depth);
                sb.Append("while ");
                HandleExpression(whileStmt.Condition, sb);
                sb.AppendLine(":");
                HandleStatementSyntax(whileStmt.Statement, sb, depth + 1);
            }
            else if (stmt.GetType() == typeof(ForEachStatementSyntax))
            {
                var foreachStmt = stmt as ForEachStatementSyntax;
                sb.AppendTabs(depth);
                sb.Append("for ");
                sb.Append(HandleSyntaxToken(foreachStmt.Identifier));
                sb.Append(" in ");
                HandleExpression(foreachStmt.Expression, sb);
                sb.AppendLine(":");
                HandleStatementSyntax(foreachStmt.Statement, sb, depth + 1);
            }
        }


        public static void HandleMethodDeclarationSyntax(MethodDeclarationSyntax decl, StringBuilder sb, int depth)
        {
            string funcName = HandleSyntaxToken(decl.Identifier);
            sb.AppendTabs(depth).Append($@"func {funcName}(");
            foreach (var param in decl.ParameterList.Parameters)
            {
                sb.Append(param.Identifier.Text);
                if (param != decl.ParameterList.Parameters.Last())
                    sb.Append(", ");
            }

            sb.Append(") -> ");
            sb.Append(HandleType(decl.ReturnType));
            sb.AppendLine(":");
            HandleStatementSyntax(decl.Body, sb, depth + 1);
        }
        public static void HandleVariableDeclarationSyntax(VariableDeclarationSyntax var, StringBuilder sb, int depth)
        {
            sb.AppendTabs(depth).Append("var ");
            foreach (var var_ in var.Variables)
            {
                sb.Append(HandleSyntaxToken(var_.Identifier));
                if (HandleType(var.Type) != "var")
                    sb.Append(":").Append(HandleType(var.Type)); // Will probably produce wrong things but gdscript does not support multi variable declarations

                if (var_.Initializer != null)
                    HandleInitialize(var_.Initializer, sb, var.Type);
                if (var_ != var.Variables.Last())
                    sb.Append(", ");
            }
            sb.AppendLine();
        }
        public static string HandleNameSyntax(NameSyntax name)
        {
            if (name.GetType() == typeof(IdentifierNameSyntax))
            {
                var ident = (IdentifierNameSyntax)name;
                return ident.Identifier.Text.ToLower();
            }
            return "UNKNOWN";
        }
        public static void HandleMemberDeclarationSyntax(MemberDeclarationSyntax decl, StringBuilder sb, int depth)
        {
            foreach (var attrs in decl.AttributeLists)
            {
                foreach (var attr in attrs.Attributes)
                {
                    //attr.Name.Ide
                    sb.AppendTabs(depth);
                    sb.Append("@");
                    sb.Append(HandleNameSyntax(attr.Name));
                    sb.AppendLine();
                }
            }

            if (decl.GetType() == typeof(PropertyDeclarationSyntax))
                HandlePropertyDeclarationSyntax(decl as PropertyDeclarationSyntax, sb, depth);
            else if (decl.GetType() == typeof(MethodDeclarationSyntax))
                HandleMethodDeclarationSyntax(decl as MethodDeclarationSyntax, sb, depth);
            else if (decl.GetType() == typeof(FieldDeclarationSyntax))
            {
                var fieldDecl = decl as FieldDeclarationSyntax;
                HandleVariableDeclarationSyntax(fieldDecl.Declaration, sb, depth);
            }
            sb.AppendLine();
            //Console.WriteLine(decl);

        }

        public static void HandleClassDecl(ClassDeclarationSyntax class_, StringBuilder sb, int depth)
        {
            // class decl +extends baseclass https://docs.godotengine.org/fr/stable/tutorials/scripting/gdscript/gdscript_basics.html

            if (class_.BaseList != null)
            {
                var firstBase = class_.BaseList.Types.First();
                if (firstBase != null)
                    sb.AppendLine($@"extends {HandleType(firstBase.Type)}");
            }

            if (!class_.Identifier.Text.Contains("Manager"))
                sb.AppendLine($@"class_name {class_.Identifier.Text}"); // No class name on singletons
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
                if (member.GetType() == typeof(NamespaceDeclarationSyntax)) // We ignore namespaces
                    HandleDecls((member as NamespaceDeclarationSyntax).Members, sb, depth);
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
                    var gdScriptPath = file.Replace(".cs", ".gd");
                    Console.WriteLine(gdScriptPath);
                    var gdScriptCode = TranspileFile(file);
                    File.WriteAllText(gdScriptPath, gdScriptCode);

                    var origBack = Console.BackgroundColor;
                    Console.WriteLine();
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write("=======================================================");
                    Console.BackgroundColor = origBack;
                    Console.WriteLine();
                    Console.WriteLine(gdScriptCode);
                }
            }
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                BrowseFiles(dir);
            }
        }
        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                if (!dirPath.Contains(".git"))
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if (!newPath.Contains(".git"))
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static void EmptyFolderExceptGit(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                if (dir.Name != ".git")
                    dir.Delete(true);
            }
        }
        public static void ReplaceCSExtToGD_CSTogdscript(string path)
        {
            var tscnContent = File.ReadAllText(path);
            var newContent = tscnContent.Replace(".cs", ".gd");
            File.WriteAllText(path, newContent);
        }
        public static void BrowseTscnFiles(string path)
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (System.IO.Path.GetExtension(file).ToLower() == ".tscn")
                {
                    Console.WriteLine("Rewrite " + file);
                    ReplaceCSExtToGD_CSTogdscript(file);
                }
            }
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                BrowseTscnFiles(dir);
            }

        }

        public static void HandleProjectFile(string inputPath, string outputPath)
        {
            var projectContent = File.ReadAllText(inputPath);
            var newContent = projectContent.Replace("\"C#\",", "");
            File.WriteAllText(outputPath, newContent);
            ReplaceCSExtToGD_CSTogdscript(outputPath);
        }
        public static void BrowseFilesRemoveCS(string path)
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (System.IO.Path.GetExtension(file) == ".cs")
                {
                    File.Delete(file);
                }
            }
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                BrowseFilesRemoveCS(dir);
            }
        }
        public static void HandleProgram()
        {
            string sourcePath
                = "C:\\Users\\sebas\\Documents\\Projects\\ldjam-54-silver";
            string folderPath = "C:\\Users\\sebas\\Documents\\Projects\\ldjam-54-silver-gdscript";
            EmptyFolderExceptGit(folderPath);
            CopyFilesRecursively(sourcePath, folderPath);


            BrowseFiles(folderPath);
            BrowseTscnFiles(folderPath);

            HandleProjectFile($"{sourcePath}\\project.godot", $"{folderPath}\\project.godot");
            try
            {
                File.Copy($"{sourcePath}\\.gitignore", $"{folderPath}\\.gitignore");
            }
            catch (Exception ex) { }

            BrowseFilesRemoveCS(folderPath);
        }
    }
}
