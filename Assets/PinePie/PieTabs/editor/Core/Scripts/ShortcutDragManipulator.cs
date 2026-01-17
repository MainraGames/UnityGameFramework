// Copyright (c) 2025 PinePie. All rights reserved.

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace PinePie.PieTabs
{
    public class ShortcutDragManipulator : PointerManipulator
    {
        private readonly Object objectToDrag;
        private Vector2 startPos;
        private bool dragging;
    
        public ShortcutDragManipulator(Object obj)
        {
            objectToDrag = obj;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }
    
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }
    
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }
    
        void OnPointerDown(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt)) return;
    
            startPos = evt.position;
            dragging = true;
            target.CapturePointer(evt.pointerId);
        }
    
        void OnPointerMove(PointerMoveEvent evt)
        {
            if (!dragging || !target.HasPointerCapture(evt.pointerId)) return;
    
            float dragThreshold = 10f;
            if (((Vector2)evt.position - startPos).sqrMagnitude > dragThreshold * dragThreshold)
            {
                if (objectToDrag != null)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { objectToDrag };
                    DragAndDrop.StartDrag("Dragging: " + objectToDrag.name);
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
    
                dragging = false;
                target.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            }
        }
    
        void OnPointerUp(PointerUpEvent evt)
        {
            if (dragging && CanStopManipulation(evt))
            {
                dragging = false;
                target.ReleasePointer(evt.pointerId);
            }
        }
    }
    
}
#endif