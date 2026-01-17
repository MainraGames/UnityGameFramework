using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace Akiba.SOCreator
{
    public class AkibaSOItem
    {
        public AkibaSOCreator.SupportedTypes type = AkibaSOCreator.SupportedTypes.String;
        public string itemName;
        public UnityEngine.Object customType;
        public Type customTypeName;
    }

    public class AkibaSOCreator : EditorWindow
    {
        string internalName = "NewScriptableObject";

        string defaultName;
        string publicName;
        string menuName = "ScriptableObjects";

        public enum SupportedTypes { String, Bool, Int, Float, Vector2, Vector3, GameObject, CustomType }

        public List<AkibaSOItem> variables;

        GUIStyle labelCentredBold;
        GUIStyle labelCentred;
        GUIStyle btnSmall;

        Vector2 _scrollPosition;

        [MenuItem("Tools/Akiba/SOC/ScriptableObject Creator", false, 200)]
        static void Init()
        {
            EditorWindow soc = GetWindow(typeof(AkibaSOCreator), false, "ScriptableObject Creator");
            soc.Show();
        }

        void Awake()
        {
            CreateNew();
        }

        void OnGUI()
        {
            this.minSize = new Vector2(700, 700);
            this.maxSize = new Vector2(700, 700);

            labelCentredBold = new GUIStyle("Label");
            labelCentredBold.fontStyle = FontStyle.Bold;
            labelCentredBold.alignment = TextAnchor.MiddleCenter;
            labelCentredBold.richText = true;

            labelCentred = new GUIStyle("Label");
            labelCentred.alignment = TextAnchor.MiddleCenter;
            labelCentred.richText = true;

            btnSmall = new GUIStyle(EditorStyles.toolbarButton);
            btnSmall.fixedHeight = 20;
            btnSmall.fixedWidth = 30;
            btnSmall.padding = new RectOffset(0, 0, 0, 0);
            btnSmall.margin = new RectOffset(0, 4, 2, 0);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
            {
                if (GUILayout.Button("New", EditorStyles.toolbarButton))
                {
                    if (EditorUtility.DisplayDialog("Create New ScriptableObject?", "Doing so will permanently erase any changes made.", "Ok", "Cancel"))
                    {
                        CreateNew();
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label("Akiba ScriptableObject Creator", labelCentred);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    if (!CheckInternalSpaces())
                    {
                        if (!CheckVariableSpaces())
                        {
                            if (EditorUtility.DisplayDialog("Alert", "Do NOT rename the file name given to this file in the save panel.", "Ok"))
                            {
                                string path = EditorUtility.SaveFilePanel("Save Generated ScriptableObject (DO NOT RENAME, MUST match Internal Name)", "", internalName, "cs");
                                
                                if (path != null)
                                {
                                    SaveNewSO(path);
                                }
                            }
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("Error", "Variables contain spaces.", "Repair Variable Names", "Ok"))
                            {
                                RepairVariableSpaces();
                                GUIUtility.hotControl = 0;
                                GUIUtility.keyboardControl = 0;
                            }
                        }
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("Error", "Internal Name contains spaces.", "Repair Internal Name", "Ok"))
                        {
                            internalName = RepairInternalSpaces();
                            GUIUtility.hotControl = 0;
                            GUIUtility.keyboardControl = 0;
                        }
                    }
                }
            }

            DrawUI();
        }

        void DrawUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawOptions();

            DrawVariables();

            EditorGUILayout.EndScrollView();
        }

        bool CheckInternalSpaces()
        {
            //Check Internal Name for Spaces
            return internalName.Contains(" ");
        }

        string RepairInternalSpaces()
        {
            //Remove Spaces from Internal Name
            return internalName.Replace(" ", "");
        }

        bool CheckVariableSpaces()
        {
            //Check Variables for Spaces
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].itemName.Contains(" "))
                {
                    return true;
                }
            }

            return false;
        }

        void RepairVariableSpaces()
        {
            //Convert Variables to Camel Case
            for (int i = 0; i < variables.Count; i++)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                char[] varName = textInfo.ToTitleCase(variables[i].itemName).Replace(" ", "").ToCharArray();
                varName[0] = char.ToLower(varName[0]);
                variables[i].itemName = new string(varName);
            }
        }

        void DrawOptions()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("ScriptableObject Options", labelCentredBold);
                    internalName = EditorGUILayout.TextField(new GUIContent("Internal Name", "The internal class name"), internalName);
                    GUILayout.Label("This name MUST not contain spaces or special characters", labelCentred);
                }

                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("Editor Options", labelCentredBold);

                    defaultName = EditorGUILayout.TextField(new GUIContent("Object Name", "The default file name"), defaultName);
                    publicName = EditorGUILayout.TextField(new GUIContent("Default Name", "The name in the Create menu"), publicName);
                    menuName = EditorGUILayout.TextField(new GUIContent("Menu Name", "The Create menu path"), menuName);
                }

                EditorGUILayout.Space();

                GUILayout.Label(string.Format("This ScriptableObject can be created under:\n\"<color=#ffac00>Create/{0}</color>\"    and will be named    \"<color=#ffac00>{1}</color>\"    by default", (menuName + "/" + publicName), defaultName), labelCentred);
            }
        }

        void DrawVariables()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.Label("Variables", labelCentredBold);

                if (variables != null && variables.Count >= 0)
                {
                    for (int i = 0; i < variables.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                        {
                            GUILayout.Label(i.ToString(), GUILayout.Width(40));

                            GUILayout.Label("Name");
                            variables[i].itemName = EditorGUILayout.TextField(variables[i].itemName);
                            GUILayout.Label("Type");
                            variables[i].type = (SupportedTypes)EditorGUILayout.EnumPopup(variables[i].type);

                            if (variables[i].type == SupportedTypes.CustomType)
                            {
                                variables[i].customType = EditorGUILayout.ObjectField("Target", variables[i].customType, typeof(UnityEngine.Object), false);
                            }

                            GUILayout.FlexibleSpace();

                            GUI.enabled = (i > 0) ? true : false;

                            if (GUILayout.Button(new GUIContent("\u25B2", "Move up in list"), btnSmall))
                            {
                                Move(variables[i], i, i - 1);
                            }

                            GUI.enabled = true;

                            if (GUILayout.Button(new GUIContent("\u2716", "Delete from list"), btnSmall))
                            {
                                variables.RemoveAt(i);
                            }

                            GUI.enabled = (i < (variables.Count - 1)) ? true : false;

                            if (GUILayout.Button(new GUIContent("\u25BC", "Move down in list"), btnSmall))
                            {
                                Move(variables[i], i, i + 1);
                            }
                        }
                    }
                }

                GUI.enabled = true;

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    if (GUILayout.Button("Documentation", EditorStyles.toolbarButton))
                    {
                        AkibaSOCDoc.Init();
                    }

                    GUILayout.FlexibleSpace();

                    if (variables != null && variables.Count <= 0)
                    {
                        GUILayout.Label("There are no variables in this ScriptableObject, please click the \"Add Variable\" button", labelCentred);
                    }
                    else if (variables == null)
                    {
                        GUILayout.Label("Editor recompiled, please select \"New\" from the top-left menu", labelCentred);
                    }

                    GUILayout.FlexibleSpace();

                    if (variables != null)
                    {
                        if (GUILayout.Button("Add Variable", EditorStyles.toolbarButton))
                        {
                            AkibaSOItem item = new AkibaSOItem();
                            item.itemName = "variable_" + variables.Count;
                            variables.Add(item);
                        }
                    }
                }
            }
        }

        void ReadCustomTarget(UnityEngine.Object target, int index)
        {
            string tgtType;
            string tgtInType;
            bool tgtIsScript;
            bool tgtIsMonobehavior;
            bool tgtRead = true;

            tgtType = target.GetType().ToString();

            if (target.GetType() == typeof(UnityEditor.MonoScript))
            {
                tgtInType = ((MonoScript)target).GetClass().BaseType.ToString();

                tgtIsScript = true;

                if (((MonoScript)target).GetClass().IsSubclassOf(typeof(UnityEngine.MonoBehaviour)) && ((MonoScript)target).GetClass().BaseType != typeof(UnityEditor.Editor) && ((MonoScript)target).GetClass().BaseType != typeof(UnityEditor.EditorWindow))
                {
                    tgtIsMonobehavior = true;
                }
                else
                {
                    tgtIsMonobehavior = false;
                }
            }
            else
            {
                tgtIsScript = false;
                tgtIsMonobehavior = false;
            }

            tgtRead = true;

            if (tgtIsScript && tgtIsMonobehavior && tgtRead)
            {
                variables[index].customTypeName = ((MonoScript)target).GetClass();
            }
        }

        void CreateNew()
        {
            internalName = "NewScriptableObject";

            defaultName = "DefaultFileName";
            publicName = "NameInMenu";
            menuName = "ScriptableObjects";
            variables = new List<AkibaSOItem>();
        }

        void SaveNewSO(string path)
        {
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace compileNS = new CodeNamespace("");
            compileUnit.Namespaces.Add(compileNS);

            compileNS.Imports.Add(new CodeNamespaceImport("UnityEngine"));

            CodeTypeDeclaration classType = new CodeTypeDeclaration(internalName);

            classType.IsClass = true;
            classType.TypeAttributes = TypeAttributes.Public;
            classType.CustomAttributes.Add(new CodeAttributeDeclaration("CreateAssetMenu", new CodeAttributeArgument("fileName", new CodePrimitiveExpression(defaultName)), new CodeAttributeArgument("menuName", new CodePrimitiveExpression((menuName + "/" + publicName)))));
            classType.BaseTypes.Add("ScriptableObject");

            string completePath = path;

            if (completePath != null && completePath.Length > 0 && completePath != "")
            {
                classType.Attributes = MemberAttributes.Public;

                List<CodeMemberField> soVars = new List<CodeMemberField>();

                for (int i = 0; i < variables.Count; i++)
                {
                    CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement();

                    switch (variables[i].type)
                    {
                        case SupportedTypes.String:
                            cvds = new CodeVariableDeclarationStatement(
                                typeof(string),
                                variables[i].itemName);
                            break;

                        case SupportedTypes.Bool:
                            cvds = new CodeVariableDeclarationStatement(
                                typeof(bool),
                                variables[i].itemName);
                            break;

                        case SupportedTypes.Int:
                            cvds = new CodeVariableDeclarationStatement(
                                typeof(int),
                                variables[i].itemName);
                            break;

                        case SupportedTypes.Float:
                            cvds = new CodeVariableDeclarationStatement(
                                typeof(float),
                                variables[i].itemName);
                            break;

                        case SupportedTypes.Vector2:
                            cvds = new CodeVariableDeclarationStatement(
                                typeof(Vector2),
                                variables[i].itemName);
                            break;

                        case SupportedTypes.Vector3:
                            cvds = new CodeVariableDeclarationStatement(
                                typeof(Vector3),
                                variables[i].itemName);
                            break;
                        case SupportedTypes.GameObject:
                            cvds = new CodeVariableDeclarationStatement(
                                typeof(GameObject),
                                variables[i].itemName);
                            break;
                        case SupportedTypes.CustomType:
                            ReadCustomTarget(variables[i].customType, i);
                            cvds = new CodeVariableDeclarationStatement(
                                variables[i].customTypeName,
                                variables[i].itemName);
                            break;
                    }

                    CodeMemberField y = new CodeMemberField(cvds.Type, cvds.Name);
                    y.Attributes = MemberAttributes.Public;
                    y.InitExpression = cvds.InitExpression;

                    soVars.Add(y);
                }

                if (soVars != null && soVars.Count > 0)
                {
                    for (int i = 0; i < soVars.Count; i++)
                    {
                        classType.Members.Add(soVars[i]);
                    }
                }

                compileNS.Types.Add(classType);

                if (File.Exists(completePath))
                {
                    File.Delete(completePath);
                }

                CodeGeneratorOptions options = new CodeGeneratorOptions();
                options.IndentString = "    ";
                options.BracingStyle = "C";

                CSharpCodeProvider cCompiler = new CSharpCodeProvider();
                using (StreamWriter writer = new StreamWriter(completePath))
                {
                    cCompiler.GenerateCodeFromCompileUnit(compileUnit, writer, options);
                }
                AssetDatabase.Refresh();
            }
        }

        public void Move(AkibaSOItem it, int oldPos, int newPos)
        {
            AkibaSOItem data = it;
            variables.RemoveAt(oldPos);
            variables.Insert(newPos, it);
        }
    } 
}