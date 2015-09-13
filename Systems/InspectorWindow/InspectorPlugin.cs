﻿using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Unity;
using Invert.Data;
using Invert.IOC;
using UnityEditor;
using UnityEngine;

public class InspectorPlugin : DiagramPlugin
    , IDrawInspector
    , IDrawExplorer
    , IDataRecordPropertyChanged
    , IDataRecordInserted
    , IDataRecordRemoved
    , IGraphSelectionEvents
    , INothingSelectedEvent
    , IWorkspaceChanged
    , IToolbarQuery
{
    private bool _graphsOpen;
    private static GUIStyle _item5;
    private static GUIStyle _item4;

    public override void Initialize(UFrameContainer container)
    {
        base.Initialize(container);

    }

    public override void Loaded(UFrameContainer container)
    {
        base.Loaded(container);
        Repository = container.Resolve<IRepository>();
        WorkspaceService = container.Resolve<WorkspaceService>();
    }

    public WorkspaceService WorkspaceService { get; set; }

    public IRepository Repository { get; set; }

    public void DrawInspector(Rect rect)
    {
     
        if (Groups == null) return;
        foreach (var group in Groups)
        {
            if (GUIHelpers.DoToolbarEx(group.Key))
            {
                foreach (var item in group)
                {
                    var d = InvertGraphEditor.PlatformDrawer as UnityDrawer;
                    d.DrawInspector(item, EditorStyles.label);
                }
            }
        }

       
     
    }

    public void UpdateItems()
    {
        if (WorkspaceService == null) return;
        if (WorkspaceService.CurrentWorkspace == null) return;

        Items =
            WorkspaceService.CurrentWorkspace.Graphs.SelectMany(p => p.NodeItems.OrderBy(x=>x.Name))
                .OfType<GenericNode>()
                .GroupBy(p => p.Graph.Name)
                .OrderBy(p => p.Key).ToArray();

        
    }

    public IGrouping<string, GenericNode>[] Items { get; set; }

    public void PropertyChanged(IDataRecord record, string name, object previousValue, object nextValue)
    {
     
        UpdateItems();
        if (uFrameInspectorWindow.Instance != null)
            uFrameInspectorWindow.Instance.Repaint();
    }
    public virtual IEnumerable<PropertyFieldViewModel> GetInspectorOptions(object obj)
    {
        if (obj == null) yield break;
        foreach (var item in obj.GetPropertiesWithAttribute<InspectorProperty>())
        {
            var property = item.Key;
            var attribute = item.Value;
            var fieldViewModel = new PropertyFieldViewModel()
            {
                Name = property.Name,
            };
            fieldViewModel.Getter = () => property.GetValue(obj, null);
            fieldViewModel.Setter = _ => property.SetValue(obj, _, null);
            fieldViewModel.InspectorType = attribute.InspectorType;
            fieldViewModel.Type = property.PropertyType;
            fieldViewModel.DataObject = obj;
            fieldViewModel.CustomDrawerType = attribute.CustomDrawerType;
            fieldViewModel.CachedValue = fieldViewModel.Getter();
            yield return fieldViewModel;
        }
    }
    private void UpdateSelection()
    {
        Groups = Selected.SelectMany(x => GetInspectorOptions(x)).GroupBy(p=>p.DataObject.GetType().Name).ToArray();
        

        if (uFrameInspectorWindow.Instance != null)
            uFrameInspectorWindow.Instance.Repaint();
    }

    public IGrouping<string, PropertyFieldViewModel>[] Groups { get; set; }


    public IDataRecord[] Selected { get; set; }

    public void RecordInserted(IDataRecord record)
    {
        UpdateItems(); 
        if (uFrameInspectorWindow.Instance != null)
            uFrameInspectorWindow.Instance.Repaint();
    }

    public void RecordRemoved(IDataRecord record)
    {
        UpdateItems(); if (uFrameInspectorWindow.Instance != null)
            uFrameInspectorWindow.Instance.Repaint();
    }

    public void SelectionChanged(GraphItemViewModel selected)
    {
        SelectItem(selected.DataObject as IDataRecord);
    }

    public void SelectItem(IDataRecord item)
    {
        var list = new List<IDataRecord>();

        if (WorkspaceService != null)
        {
            if (WorkspaceService.CurrentWorkspace != null)
            {
                list.Add(WorkspaceService.CurrentWorkspace);
                if (WorkspaceService.CurrentWorkspace.CurrentGraph != null)
                {
                    list.Add(WorkspaceService.CurrentWorkspace.CurrentGraph);
                }
            }
        }
        if (item != null)
        {
            list.Add(item);
        }
        Selected = list.ToArray();
        UpdateSelection();
    }
    public void DrawExplorer()
    {
        if (Repository == null) return;
        if (WorkspaceService == null) return;
        if (WorkspaceService.CurrentWorkspace == null) return;
        if (Items == null) UpdateItems();
        
//        if (GUIHelpers.DoToolbarEx("Explorer"))
//        {
//            
//            EditorGUI.indentLevel++;
//            foreach (var group in Items)
//            {
//                //EditorPrefs.SetBool(group.Key, EditorGUILayout.Foldout(EditorPrefs.GetBool(group.Key), group.Key));
//                if (GUIHelpers.DoToolbarEx(group.Key) ) //EditorPrefs.GetBool(group.Key))
//                {
//                    EditorGUI.indentLevel++;
//                    foreach (var node in group)
//                    {
//                        EditorGUILayout.BeginHorizontal();
//                        GUILayout.Space(EditorGUI.indentLevel * 15f);
//                        var selected = Selected != null && Selected.Identifier == node.Identifier;
//                        if (GUILayout.Button(node.Name, selected ? Item5 : Item4))
//                        {
//                            Selected = node;
//                            UpdateSelection(null);
//                        } EditorGUILayout.EndHorizontal();
//                    }
//                    EditorGUI.indentLevel--;
//                }
//
//            } EditorGUI.indentLevel--;
//        }
    }
    public static GUIStyle Item4
    {
        get
        {
            if (_item4
                == null)
                _item4 = new GUIStyle
                {
                    normal = { background = ElementDesignerStyles.GetSkinTexture("Item4"), textColor = Color.white },
                    active = { background = ElementDesignerStyles.GetSkinTexture("EventButton"), textColor = Color.white },
                    stretchHeight = true,
                    stretchWidth = true,
                    fontSize = 12,
                    fixedHeight = 20f,
                    padding = new RectOffset(5, 0, 0, 0),
                    alignment = TextAnchor.MiddleLeft
                }.WithFont("Verdana", 12);

            return _item4;
        }
    }
    public static GUIStyle Item5
    {
        get
        {
            if (_item5
                == null)
                _item5= new GUIStyle
                {
                    normal = { background = ElementDesignerStyles.GetSkinTexture("Item1"), textColor = Color.white },
                    active = { background = ElementDesignerStyles.GetSkinTexture("EventButton"), textColor = Color.white },
                    stretchHeight = true,
                    stretchWidth = true,
                    padding = new RectOffset(5,0,0,0),
                    fontSize = 12,
                    fixedHeight = 20f,
                    alignment = TextAnchor.MiddleLeft
                }.WithFont("Verdana", 12);

            return _item5;
        }
    }
    public void WorkspaceChanged(Workspace workspace)
    {
        SelectItem(null);
    }

    public void NothingSelected()
    {
        SelectItem(null);
    }
    static bool IsWindowOpen<WindowType>() where WindowType : EditorWindow
    {
        WindowType[] windows = Resources.FindObjectsOfTypeAll<WindowType>();
        return windows != null && windows.Length > 0;

    }
    public void QueryToolbarCommands(ToolbarUI ui)
    {
        var isOpen = IsWindowOpen<uFrameInspectorWindow>();
        ui.AddCommand(new ToolbarItem()
        {
            Title = "Inspector/Issues",
            Checked = isOpen,
            Position = ToolbarPosition.BottomRight,
            Command = new LambdaCommand("Show", () =>
            {
                var window = EditorWindow.GetWindow<uFrameInspectorWindow>();
                if (isOpen)
                {
                    window.Close();
                }
            })
        }); 
    }
}