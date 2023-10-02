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
        static string HandleSyntaxToken(SyntaxToken token, bool isNotType = false)
        {
            Dictionary<string, string> godotFunctions = new Dictionary<string, string>()
            {

                {"GetOverlappingAreas", "get_overlapping_areas" },
                {"ResourcePath", "resource_path" },
                {"Playing", "playing" },
                {"SetCollisionMaskValue", "set_collision_mask_value" },
                {"GetFrameTexture", "get_frame_texture" },
                {"SpriteFrames", "sprite_frames" },
                {"Set", "set" },
                {"IsZeroApprox", "is_zero_approx" },
                {"Basis", "basis" },
                {"BodyEntered", "body_entered" },
                {"Stream", "stream" },
                {"HasOverlappingAreas", "has_overlapping_areas" },
                {"IsOnFloor", "is_on_floor" },
                {"Stop", "stop" },
                {"MaterialOverride", "material_override" },
                {"Frame", "frame" },
                {"HasOverlappingBodies", "has_overlapping_bodies" },
                {"GetOverlappingBodies", "get_overlapping_bodies" },
                {"GetTicksMsec", "get_ticks_msec" },
                {"VolumeDb", "volume_db" },
                {"AddRange", "append_array" },
                {"GetNodesInGroup", "get_nodes_in_group" },
                {"Select", "map" }, // Replace c# array Select to gdscript map
                {"GetParent", "get_parent" },
                { "FindChild", "find_child" },
                { "ProcessMode", "process_mode" },
                { "Transform", "transform" }, // Meh
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
                {"_Input", "_input" },
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
                { "Forward", "FORWARD" },
                { "Backward", "BACKWARD" },
                { "Texture", "texture" },

                { "Randi", "randi"},

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
            res = res.Replace("@", "at"); // GDScript interprets @ but not C#
            if (isNotType && res.ToLower().Contains("enum"))
                res = res + "." + res; // dirty hack to handle enum export easily
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
                HandleExpression(arg.Expression, sb, 0);
                if (arg != list.Arguments.Last())
                    sb.Append(", ");
            }
        }
        // Checks if contains "" (not ideal but enough for now)
        // Would require a semantic check
        static bool IsStringExpression(string text)
        {
            return text.Contains("\"");
        }

        public static async void HandleExpression(ExpressionSyntax expr, StringBuilder sb, int depth, TypeSyntax inferedType = null)
        {
            if (expr == null) return;

            Dictionary<string, string> mapUnaryOperators = new Dictionary<string, string>()
                {
                    {"++", " += 1" },
                    {"--", " -= 1" },
                };

            var strExpr = expr.ToString();

            if (strExpr.ToString().Contains("base."))
                return; // In godot 4 we do not call base methods this is done auto
            //if (strExpr.ToString() == "Transform.Basis")
            //{
            //    sb.Append("self.Transform.Basis");
            //    return;
            //}

            if (expr.GetType() == typeof(InvocationExpressionSyntax))
            {
                var invocExpr = expr as InvocationExpressionSyntax;
                StringBuilder sbInvocExpr = new StringBuilder();
                HandleExpression(invocExpr.Expression, sbInvocExpr, depth);
                if (sbInvocExpr.ToString() == "print") // We bypass prints as it may contain some string / object concats that we cannot trivially convert
                {
                }
                else
                {
                    sb.Append(sbInvocExpr.ToString());

                    bool ignoreFunctions = expr.ToString().Contains("ToString") || expr.ToString().Contains("AsSingle");
                    if (!ignoreFunctions)
                    {
                        sb.Append("(");
                        HandleArgumentList(invocExpr.ArgumentList, sb);
                        sb.Append(")");
                    }
                }
            }
            else if (expr.GetType() == typeof(ParenthesizedLambdaExpressionSyntax))
            {
                var lambdaExpr = expr as ParenthesizedLambdaExpressionSyntax;
                sb.Append("func(");
                HandleParametersList(lambdaExpr.ParameterList, sb);
                sb.Append("): ");
                if (lambdaExpr.ExpressionBody != null)
                    HandleExpression(lambdaExpr.ExpressionBody, sb, depth);
                else
                    HandleStatementSyntax(lambdaExpr.Block, sb, depth + 1);
            }
            else if (expr.GetType() == typeof(SimpleLambdaExpressionSyntax))
            {
                var lambdaExpr = expr as SimpleLambdaExpressionSyntax;
                sb.Append("func(");
                //lambdaExpr.
                sb.Append(HandleSyntaxToken(lambdaExpr.Parameter.Identifier, true));
                sb.Append("): ");
                if (lambdaExpr.ExpressionBody != null)
                {
                    sb.Append("return ");
                    HandleExpression(lambdaExpr.ExpressionBody, sb, depth);
                }
                else
                    HandleStatementSyntax(lambdaExpr.Block, sb, depth + 1);
            }
            else if (expr.GetType() == typeof(MemberAccessExpressionSyntax))
            {

                var accessExpr = expr as MemberAccessExpressionSyntax;

                var accessAsIdentifierName = accessExpr.Expression as IdentifierNameSyntax;
                if (accessExpr.Name.Identifier.Text == "ToString")
                {
                    sb.Append("str(");
                    HandleExpression(accessExpr.Expression, sb, depth);
                    sb.Append(")");
                }
                if (accessExpr.Name.Identifier.Text == "AsSingle")
                {
                    HandleExpression(accessExpr.Expression, sb, depth);
                }
                else if (accessAsIdentifierName != null &&
                    (accessAsIdentifierName.Identifier.Text == "GD" ||
                    accessAsIdentifierName.Identifier.Text == "Mathf" ||
                    accessAsIdentifierName.Identifier.Text == "MathF" ||
                    accessAsIdentifierName.Identifier.Text == "Math" ||
                    accessAsIdentifierName.Identifier.Text == "ResourceLoader"))
                {
                    sb.Append(HandleSyntaxToken(accessExpr.Name.Identifier, true));
                }
                else if (accessAsIdentifierName != null && accessAsIdentifierName.Identifier.Text == "ProcessModeEnum")
                {
                    sb.Append($"PROCESS_MODE_{accessExpr.Name.Identifier.Text.ToUpper()}");
                }
                else
                {
                    HandleExpression(accessExpr.Expression, sb, depth);
                    sb.Append(".");
                    sb.Append(HandleSyntaxToken(accessExpr.Name.Identifier, true));
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

                HashSet<string> signalsValues = new HashSet<string>()
                {
                    "BodyEntered"
                };

                
                Func<bool> containsSignal = () =>
                {
                    foreach (var signal in signalsValues)
                    {
                        if (strExpr.Contains(signal))
                            return true;
                    }
                    return false;
                };

                if (assignExpr.OperatorToken.ToString() == "+=" && containsSignal())
                {
                    StringBuilder sbLeft = new StringBuilder();
                    HandleExpression(assignExpr.Left, sbLeft, depth);
                    StringBuilder sbRight = new StringBuilder();
                    HandleExpression(assignExpr.Right, sbRight, depth);
                    sb.Append($"{sbLeft.ToString()}.connect({sbRight})");
                }
                else
                {
                    HandleExpression(assignExpr.Left, sb, depth);

                    sb.Append(" ").Append(assignExpr.OperatorToken).Append(" ");
                    HandleExpression(assignExpr.Right, sb, depth);
                }
            }
            else if (expr.GetType() == typeof(IdentifierNameSyntax))
            {
                var identifierExpr = expr as IdentifierNameSyntax;
                sb.Append(HandleSyntaxToken(identifierExpr.Identifier, true));
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
                sb.Append(HandleSyntaxToken(genericNameExpr.Identifier, true));
            }
            else if (expr.GetType() == typeof(ArrayCreationExpressionSyntax))
            {
                var arrayCreateExpr = expr as ArrayCreationExpressionSyntax;



                if (arrayCreateExpr.Initializer != null)
                {
                    sb.Append("[");
                    HandleExpression(arrayCreateExpr.Initializer, sb, depth);
                    sb.Append("]");
                }
                else
                {
                    var arraySize = arrayCreateExpr.Type.RankSpecifiers.First().Sizes.First();
                    StringBuilder sbSize = new StringBuilder();
                    HandleExpression(arraySize, sbSize, depth);
                    sb.Append($"createArray({sbSize.ToString()})");
                }
            }
            else if (expr.GetType() == typeof(BinaryExpressionSyntax))
            {
                var binaryExpr = expr as BinaryExpressionSyntax;
                StringBuilder sbLeft = new StringBuilder();
                HandleExpression(binaryExpr.Left, sbLeft, depth);
                var operator_ = HandleSyntaxToken(binaryExpr.OperatorToken, true);
                StringBuilder sbRight = new StringBuilder();
                HandleExpression(binaryExpr.Right, sbRight, depth);
                // Below is a dirty hack to handle string concat
                string stringifyBefore = "";
                string stringifyAfter = "";
                if (operator_ == "+" && IsStringExpression(sbLeft.ToString()) || IsStringExpression(sbRight.ToString()))
                {
                    stringifyBefore = "str(";
                    stringifyAfter = ")";
                }
                sb.Append(stringifyBefore).Append(sbLeft.ToString()).Append(stringifyAfter);
                sb.Append(" ").Append(operator_).Append(" ");
                sb.Append(stringifyBefore).Append(sbRight.ToString()).Append(stringifyAfter);
            }
            else if (expr.GetType() == typeof(PrefixUnaryExpressionSyntax))
            {
                var unaryExpr = expr as PrefixUnaryExpressionSyntax;
                sb.Append(unaryExpr.OperatorToken);
                HandleExpression(unaryExpr.Operand, sb, depth);
            }
            else if (expr.GetType() == typeof(InitializerExpressionSyntax))
            {
                var initializerExpr = expr as InitializerExpressionSyntax;
                foreach (var val in initializerExpr.Expressions)
                {
                    HandleExpression(val, sb, depth);
                    if (val != initializerExpr.Expressions.Last())
                        sb.Append(", ");
                }
            }
            else if (expr.GetType() == typeof(ObjectCreationExpressionSyntax))
            {
                var objCreateExpr = expr as ObjectCreationExpressionSyntax;
                //sb.Append("new ");
                var typeText = HandleType(objCreateExpr.Type);
                if (typeText == "Array")
                {
                    sb.Append("[]");
                }
                else if (typeText == "Dictionary")
                {
                    sb.Append("{");
                    if (objCreateExpr.Initializer != null)
                    {
                        foreach (var val in objCreateExpr.Initializer.Expressions)
                        {
                            if (val.GetType() == typeof(InitializerExpressionSyntax))
                            {
                                var initExpr = val as InitializerExpressionSyntax;
                                HandleExpression(initExpr.Expressions.First(), sb, depth);
                                sb.Append(": ");
                                HandleExpression(initExpr.Expressions.Last(), sb , depth);
                            }
                            if (val != objCreateExpr.Initializer.Expressions.Last())
                                sb.Append(", ");
                        }

                    }
                    sb.Append("}");
                }
                else
                {
                    sb.Append(typeText);
                    if (typeText == "Vector2" || typeText == "Vector3" || typeText == "Vector4")
                    {
                    }
                    else
                        sb.Append(".new");
                    sb.Append("(");
                    HandleArgumentList(objCreateExpr.ArgumentList, sb);
                    sb.Append(")");
                }
            }
            else if (expr.GetType() == typeof(CastExpressionSyntax))
            {
                var castExpr = expr as CastExpressionSyntax;
                sb.Append("(");
                HandleExpression(castExpr.Expression, sb, depth);
                var typeName = HandleType(castExpr.Type);
                if (!typeName.ToLower().Contains("enum")) // TODO Yeah dirty // We don't need to cast enum to int
                {
                    sb.Append(" as ");
                    sb.Append(typeName);
                }
                sb.Append(")");
            }
            else if (expr.GetType() == typeof(ParenthesizedExpressionSyntax))
            {
                var parenthExpr = expr as ParenthesizedExpressionSyntax;
                sb.Append("(");
                HandleExpression(parenthExpr.Expression, sb, depth);
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
                HandleExpression(elementAccessExpr.Expression, sb, depth);
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
                HandleExpression(postFixExpr.Operand, sb, depth);
                string newOp = "";
                if (mapUnaryOperators.TryGetValue(postFixExpr.OperatorToken.Text, out newOp))
                    sb.Append(newOp);
                else
                    sb.Append(postFixExpr.OperatorToken);
            }
            else if (expr.GetType() == typeof(PrefixUnaryExpressionSyntax))
            {
                var postFixExpr = expr as PostfixUnaryExpressionSyntax;

                HandleExpression(postFixExpr.Operand, sb, depth);
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
                    HandleExpression(awaitInvoc.ArgumentList.Arguments.First().Expression, sb, depth);
                    sb.Append(".");
                    var access = awaitInvoc.ArgumentList.Arguments.Last().ToString().Replace("\"", "");
                    sb.Append(access);
                }
            }
            else if (expr.GetType() == typeof(ConditionalExpressionSyntax))
            {
                var condExpr = expr as ConditionalExpressionSyntax;
                HandleExpression(condExpr.WhenTrue, sb, depth);
                sb.Append(" if ");
                HandleExpression(condExpr.Condition, sb, depth);
                sb.Append(" else ");
                HandleExpression(condExpr.WhenFalse, sb, depth);
            }
            else if (expr.GetType() == typeof(ConditionalAccessExpressionSyntax))
            {
                // TODO Too hard to handle now
                //var condExpr = (ConditionalAccessExpressionSyntax)expr;
                //sb.Append($"coalesce___({condExpr.Expression}, ");
                //HandleExpression(condExpr.WhenNotNull, sb, depth);
                //sb.Append(")");
            }
            else
            {

            }
        }
        public static void HandleInitialize(EqualsValueClauseSyntax equals, StringBuilder sb, TypeSyntax inferedType)
        {
            sb.Append(" = ");
            HandleExpression(equals.Value, sb, 0, inferedType);
        }

        static string HandleVarDecl(SyntaxToken name, TypeSyntax type, EqualsValueClauseSyntax? init)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($@"var {HandleSyntaxToken(name)}");
            var typeText = HandleType(type);
            if (typeText.ToLower().Contains("enum"))
            {
                sb.Append(": int");
            }
            else if (typeText != "var")
                sb.Append($": {typeText}"); // TODO array init

            StringBuilder sbInit = new StringBuilder();
            if (init != null)
            {
                HandleInitialize(init, sbInit, type);
                if (typeText == "Array" && sbInit.ToString().EndsWith("null")) // Cannot init array to null in gdscript
                {
                    sbInit.Clear();
                    sbInit.Append("= []");
                }

            }
            sb.Append(sbInit.ToString());
            return sb.ToString();
        }
        public static void HandleAccessor(string accessor, ExpressionSyntax body, StringBuilder sb, int depth)
        {

            sb.AppendLine("");
            sb.AppendTabs(depth + 1).Append($"{accessor}:").AppendLine();
            sb.AppendTabs(depth + 2);
            if (accessor == "get")
                sb.Append("return ");
            HandleExpression(body, sb, depth + 1);
        }
        public static void HandlePropertyDeclarationSyntax(PropertyDeclarationSyntax decl, StringBuilder sb, int depth)
        {
            sb.Append(HandleVarDecl(decl.Identifier, decl.Type, decl.Initializer));
            if (decl.AccessorList != null)
            {
                sb.Append(":");
                foreach (var accessor in decl.AccessorList.Accessors)
                {
                    var outputAccessor = accessor.Keyword.ToString();
                    if (outputAccessor == "set")
                        outputAccessor = "set(value)";
                    HandleAccessor(outputAccessor, accessor.ExpressionBody.Expression, sb, depth);
                    
                }
            }
            if (decl.ExpressionBody != null)
            {
                sb.Append(":");
                HandleAccessor("get", decl.ExpressionBody.Expression, sb, depth);
            }
            
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
                HandleExpression(exprStmt.Expression, sb, depth);
                sb.AppendLine();
            }
            else if (stmt.GetType() == typeof(ReturnStatementSyntax))
            {
                var retStmt = stmt as ReturnStatementSyntax;
                sb.AppendTabs(depth).Append("return ");
                HandleExpression(retStmt.Expression, sb, depth);
                sb.AppendLine();
            }
            else if (stmt.GetType() == typeof(IfStatementSyntax))
            {
                var ifStmt = stmt as IfStatementSyntax;
                sb.AppendTabs(depth);
                sb.Append("if ");
                HandleExpression(ifStmt.Condition, sb, depth);
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
                HandleExpression(switchStmt.Expression, sb, depth);
                sb.AppendLine(":");
                foreach (var clause in switchStmt.Sections)
                {
                    sb.AppendTabs(depth + 1);
                    foreach (var label in clause.Labels)
                    {
                        if (label.GetType() == typeof(CaseSwitchLabelSyntax))
                        {
                            var caseSwitch = label as CaseSwitchLabelSyntax;
                            HandleExpression(caseSwitch.Value, sb, depth);
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

                if (stmt.Parent.GetType() != typeof(SwitchSectionSyntax)) // No breaks in python switches
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
                HandleExpression(forStmt.Condition, sb, depth);
                sb.AppendLine(":");
                HandleStatementSyntax(forStmt.Statement, sb, depth + 1);
                foreach (var incrementor in forStmt.Incrementors)
                {
                    sb.AppendTabs(depth + 1);
                    HandleExpression(incrementor, sb, depth + 1);
                    sb.AppendLine();
                }
            }
            else if (stmt.GetType() == typeof(WhileStatementSyntax))
            {
                var whileStmt = stmt as WhileStatementSyntax;
                sb.AppendTabs(depth);
                sb.Append("while ");
                HandleExpression(whileStmt.Condition, sb, depth);
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
                HandleExpression(foreachStmt.Expression, sb, depth + 1);
                sb.AppendLine(":");
                HandleStatementSyntax(foreachStmt.Statement, sb, depth + 1);
            }
            else
            {

            }
        }

        public static void HandleParametersList(ParameterListSyntax params_, StringBuilder sb)
        {
            foreach (var param in params_.Parameters)
            {
                sb.Append(HandleSyntaxToken(param.Identifier));
                if (param.Default != null)
                {
                    sb.Append(":");
                    HandleInitialize(param.Default, sb, null);
                }
                if (param != params_.Parameters.Last())
                    sb.Append(", ");
            }
        }

        /// <summary>
        /// Handle MethodDeclarationSyntax and Contructor
        /// </summary>
        /// <param name="decl"></param>
        /// <param name="sb"></param>
        /// <param name="depth"></param>
        public static void HandleFunctionDeclarationSyntax(ParameterListSyntax params_, BlockSyntax? body, ArrowExpressionClauseSyntax? exprBody, StringBuilder sb, int depth, string name = null, TypeSyntax returnType = null)
        {
            string funcName = name;
            if (funcName == null)
                funcName = "_init";
            sb.AppendTabs(depth).Append($@"func {funcName}(");
            HandleParametersList(params_, sb);

            sb.Append(")");
            if (name != null)
            {
                sb.Append(" -> ");
                sb.Append(HandleType(returnType));
            }
            sb.AppendLine(":");
            if (body != null)
                HandleStatementSyntax(body, sb, depth + 1);
            else
            {
                HandleStatementSyntax(SyntaxFactory.ReturnStatement(exprBody.Expression), sb, depth + 1);
            }
            sb.AppendTabs(depth + 1).Append("pass"); // To avoid some edge cases causing empty functions
        }

        public static void HandleMethodDeclarationSyntax(MethodDeclarationSyntax decl, StringBuilder sb, int depth)
        {
            string funcName = HandleSyntaxToken(decl.Identifier);
            HandleFunctionDeclarationSyntax(decl.ParameterList, decl.Body, decl.ExpressionBody, sb, depth, funcName, decl.ReturnType);
        }
        public static void HandleVariableDeclarationSyntax(VariableDeclarationSyntax var, StringBuilder sb, int depth)
        {
            sb.AppendTabs(depth);
            foreach (var var_ in var.Variables)
            {
                // Will probably produce wrong things but gdscript does not support multi variable declarations
                sb.Append(HandleVarDecl(var_.Identifier, var.Type, var_.Initializer));

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
            else if (decl.GetType() == typeof(ConstructorDeclarationSyntax))
            {
                var ctorDecl = decl as ConstructorDeclarationSyntax;
                HandleFunctionDeclarationSyntax(ctorDecl.ParameterList, ctorDecl.Body, ctorDecl.ExpressionBody, sb, depth);
            }
            else
            {

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
        public static void HandleEnumDecl(EnumDeclarationSyntax enum_, StringBuilder sb, int depth)
        {
            var identifier = enum_.Identifier.Text;// HandleSyntaxToken(enum_.Identifier.Text); // We do not want the HandleSyntaxToken here as we have a dirty trick to handle enum exports
            // https://ask.godotengine.org/40827/how-to-declare-a-global-named-enum
            sb.AppendLine($"class_name {identifier}"); // To have "global" enums (but it requires having one enum per file)
            sb.Append($"enum {identifier} {{");
            foreach (var decl in enum_.Members)
            {

                sb.Append($"{HandleSyntaxToken(decl.Identifier)}");
                if (decl != enum_.Members.Last())
                    sb.Append(", ");
            }
            sb.AppendLine("}");
        }

        public static void HandleDecls(SyntaxList<MemberDeclarationSyntax> decls_, StringBuilder sb, int depth)
        {
            var decls = decls_.ToList();
            decls.Sort((a, b) =>
            {
                return a.GetType() == typeof(EnumDeclarationSyntax) ? 1 : 0;
            });
            foreach (var member in decls)
            {
                if (member.GetType() == typeof(ClassDeclarationSyntax))
                    HandleClassDecl(member as ClassDeclarationSyntax, sb, depth);
                if (member.GetType() == typeof(NamespaceDeclarationSyntax)) // We ignore namespaces
                    HandleDecls((member as NamespaceDeclarationSyntax).Members, sb, depth);
                if (member.GetType() == typeof(EnumDeclarationSyntax))
                    HandleEnumDecl(member as EnumDeclarationSyntax, sb, depth);
            }

        }

        public static string TranspileFile(string path)
        {
            var programText = File.ReadAllText(path);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            StringBuilder sb = new StringBuilder();
            HandleDecls(root.Members, sb, 0);
            sb.AppendLine("func createArray(size___):");
            sb.Append("\t").AppendLine("var arr = []");
            sb.Append("\t").AppendLine("arr.resize(size___)");
            sb.Append("\t").AppendLine("return arr");
            sb.AppendLine("");

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
                = "E:\\z0rg\\Projects\\Perso\\Ludum54\\ldjam-54-silver";
            string folderPath = "E:\\z0rg\\Projects\\Perso\\Ludum54\\ldjam-54-silver-gdscript";
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
