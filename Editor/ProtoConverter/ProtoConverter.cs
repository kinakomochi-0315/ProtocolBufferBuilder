using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.ProtoConverter
{
    public class ProtoConverter : EditorWindow
    {
        private const string PROTOC_PATH = "Assets/Editor/ProtoConverter/src/protoc.exe";
        private const string GRPC_PLUGIN_PATH = "Assets/Editor/ProtoConverter/src/grpc_csharp_plugin.exe";

        private static string _protoFilePath;
        private static string _outputFolderPath;

        [MenuItem("Window/Protocol Buffers Builder")]
        private static void Open()
        {
            var window = GetWindow<ProtoConverter>();
            window.titleContent = new GUIContent("Protocol Buffers Builder");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("変換元ファイル選択");

                EditorGUILayout.BeginHorizontal();
                    _protoFilePath = EditorGUILayout.TextField(_protoFilePath);
                    if (GUILayout.Button("参照"))
                    {
                        var path = EditorUtility.OpenFilePanel("ファイル選択", "", "proto");

                        if (!string.IsNullOrEmpty(path))
                        {
                            _protoFilePath = path;

                            // フォルダが指定されていない場合はファイルのあるフォルダを出力先に設定
                            if (string.IsNullOrEmpty(_outputFolderPath))
                            {
                                _outputFolderPath = Path.GetDirectoryName(path);
                            }
                        }
                    }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("変換先フォルダ選択");

                EditorGUILayout.BeginHorizontal();
                    _outputFolderPath = EditorGUILayout.TextField(_outputFolderPath);
                    if (GUILayout.Button("参照"))
                    {
                        var path = EditorUtility.OpenFolderPanel("フォルダ選択", "", "");

                        if (!string.IsNullOrEmpty(path))
                        {
                            _outputFolderPath = path;
                        }
                    }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                if (GUILayout.Button("ビルド"))
                {
                    var protoDir = Path.GetDirectoryName(_protoFilePath);
                    var protoFile = Path.GetFileName(_protoFilePath);
                    var protocPath = AssetsToAbsolutePath(PROTOC_PATH);
                    var pluginPath = AssetsToAbsolutePath(GRPC_PLUGIN_PATH);

                    Debug.Log($"protoDir: {protoDir}\nprotoFile: {protoFile}\npluginPath: {pluginPath}");

                    using var process = new System.Diagnostics.Process();

                    process.StartInfo.FileName = protocPath;
                    process.StartInfo.Arguments = $"-I {protoDir} " +
                                                  $"--plugin=protoc-gen-grpc={pluginPath} " +
                                                  $"--csharp_out={_outputFolderPath} " +
                                                  $"--grpc_out={_outputFolderPath} " +
                                                  $"{protoFile}";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    process.WaitForExit();

                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError(error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("変換が完了しました", MessageType.Info);
                    }
                }

            EditorGUILayout.EndVertical();
        }

        private static string AssetsToAbsolutePath(string assetsPath)
        {
            return assetsPath.Replace("Assets", Application.dataPath);
        }
    }
}
