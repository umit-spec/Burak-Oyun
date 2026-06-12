using UnityEditor;
using UnityEngine;

namespace BurakOyun.Editor
{
    [InitializeOnLoad]
    public static class TagSetup
    {
        static TagSetup()
        {
            EnsureTag("Player");
        }

        static void EnsureTag(string tag)
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var tags = tagManager.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;

            // Unity'nin built-in tag'leri zaten var mı kontrol
            try { GameObject.FindWithTag(tag); return; } catch { }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"[BurakOyun] '{tag}' tag'i oluşturuldu.");
        }
    }
}
