#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Schwartz.GroundObject
{
    

    public class UtilityGroundPlayer : MonoBehaviour
    {
        [MenuItem("Utility/ Ground object %g")]
        public static void GroundObjects()
        {
            foreach (Transform transform in Selection.transforms)
            {
                var hits = Physics.RaycastAll(transform.position + Vector3.up, Vector3.down, 1000f);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject == transform.gameObject)
                        continue;
                    Undo.RecordObject(transform, "Place the " + transform.name + " on the ground");
                    transform.position = hit.point;
                    break;
                }
            }

            if (Selection.transforms.Length > 0)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    } 
}

#endif
