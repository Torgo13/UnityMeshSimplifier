#region License
/*
MIT License

Copyright(c) 2017-2020 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// Contains methods for combining meshes.
    /// </summary>
    public static class MeshCombiner
    {
        #region Public Methods
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Combines an array of mesh renderers into one single mesh.
        /// </summary>
        /// <param name="rootTransform">The root transform to create the combine mesh based from, essentially the origin of the new mesh.</param>
        /// <param name="renderers">The array of mesh renderers to combine.</param>
        /// <param name="resultMaterials">The resulting materials for the combined mesh.</param>
        /// <returns>The combined mesh.</returns>
        public static Mesh CombineMeshes(Transform rootTransform, List<MeshRenderer> renderers, out Material[] resultMaterials)
        {
#if OPTIMISATION_NULL
#else
            if (rootTransform == null)
                throw new System.ArgumentNullException(nameof(rootTransform));
            else if (renderers == null)
                throw new System.ArgumentNullException(nameof(renderers));
#endif // OPTIMISATION_NULL

            var renderersLength = renderers.Count;
            var meshes = new Mesh[renderersLength];
            var transforms = new Unity.Collections.NativeArray<Matrix4x4>(renderersLength,
                Unity.Collections.Allocator.Temp,
                Unity.Collections.NativeArrayOptions.UninitializedMemory);
            var materials = new Material[renderersLength][];
            var worldToLocalMatrix = rootTransform.worldToLocalMatrix;
            for (int i = 0; i < renderersLength; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                    throw new System.ArgumentException(string.Format("The renderer at index {0} is null.", i), nameof(renderers));

                var rendererTransform = renderer.transform;
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null)
                    throw new System.ArgumentException(string.Format("The renderer at index {0} has no mesh filter.", i), nameof(renderers));
                else if (meshFilter.sharedMesh == null)
                    throw new System.ArgumentException(string.Format("The mesh filter for renderer at index {0} has no mesh.", i), nameof(renderers));
                else if (!CanReadMesh(meshFilter.sharedMesh))
                    throw new System.ArgumentException(string.Format("The mesh in the mesh filter for renderer at index {0} is not readable.", i), nameof(renderers));

                meshes[i] = meshFilter.sharedMesh;
                transforms[i] = worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                materials[i] = renderer.sharedMaterials;
            }

            return CombineMeshes(meshes, transforms, materials, out resultMaterials);
        }

        /// <summary>
        /// Combines an array of skinned mesh renderers into one single skinned mesh.
        /// </summary>
        /// <param name="rootTransform">The root transform to create the combine mesh based from, essentially the origin of the new mesh.</param>
        /// <param name="renderers">The array of skinned mesh renderers to combine.</param>
        /// <param name="resultMaterials">The resulting materials for the combined mesh.</param>
        /// <param name="resultBones">The resulting bones for the combined mesh.</param>
        /// <returns>The combined mesh.</returns>
#if OPTIMISATION
        public static Mesh CombineMeshes(Transform rootTransform, List<SkinnedMeshRenderer> renderers, out Material[] resultMaterials, out Transform[]? resultBones)
        {
            var renderersLength = renderers.Count;
            var meshes = new Mesh[renderersLength];
            var transforms = new Matrix4x4[renderersLength];
            var materials = new Material[renderersLength][];
            var bones = new Transform[renderersLength][];

            for (int i = 0; i < renderersLength; i++)
#else
        public static Mesh CombineMeshes(Transform rootTransform, SkinnedMeshRenderer[] renderers, out Material[] resultMaterials, out Transform[]? resultBones)
        {
            if (rootTransform == null)
                throw new System.ArgumentNullException(nameof(rootTransform));
            else if (renderers == null)
                throw new System.ArgumentNullException(nameof(renderers));

            var meshes = new Mesh[renderers.Length];
            var transforms = new Matrix4x4[renderers.Length];
            var materials = new Material[renderers.Length][];
            var bones = new Transform[renderers.Length][];

            for (int i = 0; i < renderers.Length; i++)
#endif // OPTIMISATION
            {
                var renderer = renderers[i];
                if (renderer == null)
                    throw new System.ArgumentException(string.Format("The renderer at index {0} is null.", i), nameof(renderers));
                else if (renderer.sharedMesh == null)
                    throw new System.ArgumentException(string.Format("The renderer at index {0} has no mesh.", i), nameof(renderers));
                else if (!CanReadMesh(renderer.sharedMesh))
                    throw new System.ArgumentException(string.Format("The mesh in the renderer at index {0} is not readable.", i), nameof(renderers));

                var rendererTransform = renderer.transform;
                meshes[i] = renderer.sharedMesh;
                transforms[i] = rendererTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                materials[i] = renderer.sharedMaterials;
                bones[i] = renderer.bones;
            }

            return CombineMeshes(meshes, transforms, materials, bones, out resultMaterials, out resultBones);
        }

        /// <summary>
        /// Combines an array of meshes into a single mesh.
        /// </summary>
        /// <param name="meshes">The array of meshes to combine.</param>
        /// <param name="transforms">The array of transforms for the meshes.</param>
        /// <param name="materials">The array of materials for each mesh to combine.</param>
        /// <param name="resultMaterials">The resulting materials for the combined mesh.</param>
        /// <returns>The combined mesh.</returns>
        public static Mesh CombineMeshes(Mesh[] meshes, System.ReadOnlySpan<Matrix4x4> transforms, Material[][] materials, out Material[] resultMaterials)
        {
#if OPTIMISATION_NULL
#else
            if (meshes == null)
                throw new System.ArgumentNullException(nameof(meshes));
            else if (transforms == null)
                throw new System.ArgumentNullException(nameof(transforms));
            else if (materials == null)
                throw new System.ArgumentNullException(nameof(materials));
#endif // OPTIMISATION_NULL

            return CombineMeshes(meshes, transforms, materials, null, out resultMaterials, out _);
        }

        /// <summary>
        /// Combines an array of meshes into a single mesh.
        /// </summary>
        /// <param name="meshes">The array of meshes to combine.</param>
        /// <param name="transforms">The array of transforms for the meshes.</param>
        /// <param name="materials">The array of materials for each mesh to combine.</param>
        /// <param name="bones">The array of bones for each mesh to combine.</param>
        /// <param name="resultMaterials">The resulting materials for the combined mesh.</param>
        /// <param name="resultBones">The resulting bones for the combined mesh.</param>
        /// <returns>The combined mesh.</returns>
        public static Mesh CombineMeshes(Mesh[] meshes, System.ReadOnlySpan<Matrix4x4> transforms, Material[][] materials, Transform[][]? bones, out Material[] resultMaterials, out Transform[]? resultBones)
        {
#if OPTIMISATION_NULL
            if (transforms.Length != meshes.Length)
#else
            if (meshes == null)
                throw new System.ArgumentNullException(nameof(meshes));
            else if (transforms == null)
                throw new System.ArgumentNullException(nameof(transforms));
            else if (materials == null)
                throw new System.ArgumentNullException(nameof(materials));
            else if (transforms.Length != meshes.Length)
#endif // OPTIMISATION_NULL
                throw new System.ArgumentException("The array of transforms doesn't have the same length as the array of meshes.", nameof(transforms));
            else if (materials.Length != meshes.Length)
                throw new System.ArgumentException("The array of materials doesn't have the same length as the array of meshes.", nameof(materials));
            else if (bones != null && bones.Length != meshes.Length)
                throw new System.ArgumentException("The array of bones doesn't have the same length as the array of meshes.", nameof(bones));

            int totalVertexCount = 0;
            int totalSubMeshCount = 0;
            for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
            {
                var mesh = meshes[meshIndex];
                if (mesh == null)
                    throw new System.ArgumentException(string.Format("The mesh at index {0} is null.", meshIndex), nameof(meshes));
                else if (!CanReadMesh(mesh))
                    throw new System.ArgumentException(string.Format("The mesh at index {0} is not readable.", meshIndex), nameof(meshes));

                totalVertexCount += mesh.vertexCount;
                totalSubMeshCount += mesh.subMeshCount;

                // Validate the mesh materials
                var meshMaterials = materials[meshIndex];
#if OPTIMISATION_NULL
                if (meshMaterials.Length != mesh.subMeshCount)
#else
                if (meshMaterials == null)
                    throw new System.ArgumentException(string.Format("The materials for mesh at index {0} is null.", meshIndex), nameof(materials));
                else if (meshMaterials.Length != mesh.subMeshCount)
#endif // OPTIMISATION_NULL
                    throw new System.ArgumentException(
                        string.Format("The materials for mesh at index {0} doesn't match the submesh count ({1} != {2}).",
                            meshIndex, meshMaterials.Length, mesh.subMeshCount), nameof(materials));

                for (int materialIndex = 0; materialIndex < meshMaterials.Length; materialIndex++)
                {
                    if (meshMaterials[materialIndex] == null)
                        throw new System.ArgumentException(string.Format("The material at index {0} for mesh at index {1} is null.", materialIndex, meshIndex), nameof(materials));
                }

                // Validate the mesh bones
                if (bones != null)
                {
                    var meshBones = bones[meshIndex];
                    if (meshBones == null)
                        throw new System.ArgumentException(string.Format("The bones for mesh at index {0} is null.", meshIndex), nameof(meshBones));

                    for (int boneIndex = 0; boneIndex < meshBones.Length; boneIndex++)
                    {
                        if (meshBones[boneIndex] == null)
                            throw new System.ArgumentException(string.Format("The bone at index {0} for mesh at index {1} is null.", boneIndex, meshIndex), nameof(meshBones));
                    }
                }
            }

            using var _0 = UnityEngine.Pool.ListPool<Vector3>.Get(out var combinedVertices);
            using var _1 = UnityEngine.Pool.ListPool<int[]>.Get(out var combinedIndices);
            if (combinedVertices.Capacity < totalVertexCount)
                combinedVertices.Capacity = totalVertexCount;
            if (combinedIndices.Capacity < totalSubMeshCount)
                combinedIndices.Capacity = totalSubMeshCount;
            List<Vector3>? combinedNormals = null;
            List<Vector4>? combinedTangents = null;
            List<Color>? combinedColors = null;
            List<BoneWeight>? combinedBoneWeights = null;
            var combinedUVs = new List<Vector4>[MeshUtils.UVChannelCount];

            using var _2 = UnityEngine.Pool.ListPool<Matrix4x4>.Get(out var usedBindposes);
            using var _3 = UnityEngine.Pool.ListPool<Transform>.Get(out var usedBones);
            using var _4 = UnityEngine.Pool.ListPool<Material>.Get(out var usedMaterials);
            using var _5 = UnityEngine.Pool.DictionaryPool<Material, int>.Get(out var materialMap);
            if (usedMaterials.Capacity < totalSubMeshCount)
                usedMaterials.Capacity = totalSubMeshCount;
            _ = materialMap.EnsureCapacity(totalSubMeshCount);

            int currentVertexCount = 0;
            for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
            {
                var mesh = meshes[meshIndex];
                var meshTransform = transforms[meshIndex];
                var meshMaterials = materials[meshIndex];
                var meshBones = (bones != null ? bones[meshIndex] : null);

                int subMeshCount = mesh.subMeshCount;
                int meshVertexCount = mesh.vertexCount;
                var meshVertices = mesh.vertices;
                var meshNormals = mesh.normals;
                var meshTangents = mesh.tangents;
                var meshUVs = MeshUtils.GetMeshUVs(mesh);
                var meshColors = mesh.colors;
                var meshBoneWeights = mesh.boneWeights;
                var meshBindposes = mesh.bindposes;

                // Transform vertices with bones to keep only one bindpose
                if (meshBones != null && meshBoneWeights != null && meshBoneWeights.Length > 0 && meshBindposes != null && meshBindposes.Length > 0 && meshBones.Length == meshBindposes.Length)
                {
                    var boneIndices = new Unity.Collections.NativeArray<int>(meshBones.Length,
                        Unity.Collections.Allocator.Temp);
                    for (int i = 0; i < meshBones.Length; i++)
                    {
                        int usedBoneIndex = usedBones.IndexOf(meshBones[i]);
                        if (usedBoneIndex == -1 || meshBindposes[i] != usedBindposes[usedBoneIndex])
                        {
                            usedBoneIndex = usedBones.Count;
                            usedBones.Add(meshBones[i]);
                            usedBindposes.Add(meshBindposes[i]);
                        }
                        boneIndices[i] = usedBoneIndex;
                    }

                    // Then we remap the bones
                    RemapBones(meshBoneWeights, boneIndices);
                    boneIndices.Dispose();
                }

                // Transforms the vertices, normals and tangents using the mesh transform
                TransformVertices(meshVertices, ref meshTransform);
                TransformNormals(meshNormals, ref meshTransform);
                TransformTangents(meshTangents, ref meshTransform);

                // Copy vertex positions & attributes
                CopyVertexPositions(combinedVertices, meshVertices);
                CopyVertexAttributes(ref combinedNormals, meshNormals, currentVertexCount, meshVertexCount, totalVertexCount, new Vector3(1f, 0f, 0f));
                CopyVertexAttributes(ref combinedTangents, meshTangents, currentVertexCount, meshVertexCount, totalVertexCount, new Vector4(0f, 0f, 1f, 1f));
                CopyVertexAttributes(ref combinedColors, meshColors, currentVertexCount, meshVertexCount, totalVertexCount, new Color(1f, 1f, 1f, 1f));
                CopyVertexAttributes(ref combinedBoneWeights, meshBoneWeights, currentVertexCount, meshVertexCount, totalVertexCount, new BoneWeight());

                for (int channel = 0; channel < meshUVs.Length; channel++)
                {
                    CopyVertexAttributes(ref combinedUVs[channel], meshUVs[channel], currentVertexCount, meshVertexCount, totalVertexCount, new Vector4(0f, 0f, 0f, 0f));
                }

                for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
                {
                    var subMeshMaterial = meshMaterials[subMeshIndex];
                    var subMeshIndices = mesh.GetTriangles(subMeshIndex, true);

                    if (currentVertexCount > 0)
                    {
                        for (int index = 0; index < subMeshIndices.Length; index++)
                        {
                            subMeshIndices[index] += currentVertexCount;
                        }
                    }

                    int existingSubMeshIndex;
                    if (materialMap.TryGetValue(subMeshMaterial, out existingSubMeshIndex))
                    {
                        combinedIndices[existingSubMeshIndex] = MergeArrays(combinedIndices[existingSubMeshIndex], subMeshIndices);
                    }
                    else
                    {
                        int materialIndex = combinedIndices.Count;
                        materialMap.Add(subMeshMaterial, materialIndex);
                        usedMaterials.Add(subMeshMaterial);
                        combinedIndices.Add(subMeshIndices);
                    }
                }

                currentVertexCount += meshVertexCount;
            }

            var resultVertices = combinedVertices;
            var resultIndices = combinedIndices.ToArray();
            var resultNormals = combinedNormals;
            var resultTangents = combinedTangents;
            var resultColors = combinedColors;
            var resultBoneWeights = (combinedBoneWeights != null ? combinedBoneWeights.ToArray() : null);
            var resultUVs = combinedUVs;
            var resultBindposes = new Unity.Collections.NativeArray<Matrix4x4>(usedBindposes.Count,
                Unity.Collections.Allocator.Temp,
                Unity.Collections.NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < resultBindposes.Length; i++)
            {
                resultBindposes[i] = usedBindposes[i];
            }
            resultMaterials = usedMaterials.ToArray();
            resultBones = (usedBones.Count > 0 ? usedBones.ToArray() : null);
            return MeshUtils.CreateMesh(resultVertices, resultIndices, resultNormals, resultTangents, resultColors, resultBoneWeights, resultUVs, resultBindposes, default);
        }
        #endregion

        #region Private Methods
        private static void CopyVertexPositions(List<Vector3> list, Vector3[] arr)
        {
#if OPTIMISATION_NULL
            if (arr.Length == 0)
#else
            if (arr == null || arr.Length == 0)
#endif // OPTIMISATION_NULL
                return;

            list.AddRange(arr);
        }

        private static void CopyVertexAttributes<T>([System.Diagnostics.CodeAnalysis.NotNull] ref List<T>? dest, List<T> src, int previousVertexCount, int meshVertexCount, int totalVertexCount, T defaultValue)
        {
#if OPTIMISATION_NULL
            if (src.Count == 0)
#else
            if (src == null || !src.Any())
#endif // OPTIMISATION_NULL
            {
                if (dest != null)
                {
                    for (int i = 0; i < meshVertexCount; i++)
                    {
                        dest.Add(defaultValue);
                    }
                }
                return;
            }

            if (dest == null)
            {
                dest = new List<T>(totalVertexCount);
                for (int i = 0; i < previousVertexCount; i++)
                {
                    dest.Add(defaultValue);
                }
            }

            dest.AddRange(src);
        }

        private static void CopyVertexAttributes<T>(ref List<T>? dest, System.ReadOnlySpan<T> src, int previousVertexCount, int meshVertexCount, int totalVertexCount, T defaultValue)
        {
            if (src == null || src.Length == 0)
            {
                if (dest != null)
                {
                    for (int i = 0; i < meshVertexCount; i++)
                    {
                        dest.Add(defaultValue);
                    }
                }
                return;
            }

            if (dest == null)
            {
                dest = new List<T>(totalVertexCount);
                for (int i = 0; i < previousVertexCount; i++)
                {
                    dest.Add(defaultValue);
                }
            }

            if (dest.Capacity < src.Length)
                dest.Capacity = src.Length;

            foreach (var value in src)
            {
                dest.Add(value);
            }
        }

        private static T[] MergeArrays<T>(T[] arr1, T[] arr2)
        {
            var newArr = new T[arr1.Length + arr2.Length];
            System.Array.Copy(arr1, 0, newArr, 0, arr1.Length);
            System.Array.Copy(arr2, 0, newArr, arr1.Length, arr2.Length);
            return newArr;
        }

        private static void TransformVertices(Vector3[] vertices, ref Matrix4x4 transform)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = transform.MultiplyPoint3x4(vertices[i]);
            }
        }

        private static void TransformNormals(Vector3[]? normals, ref Matrix4x4 transform)
        {
            if (normals == null)
                return;

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = transform.MultiplyVector(normals[i]);
            }
        }

        private static void TransformTangents(Vector4[]? tangents, ref Matrix4x4 transform)
        {
            if (tangents == null)
                return;

            Vector3 tengentDir;
            for (int i = 0; i < tangents.Length; i++)
            {
                tengentDir = transform.MultiplyVector(new Vector3(tangents[i].x, tangents[i].y, tangents[i].z));
                tangents[i] = new Vector4(tengentDir.x, tengentDir.y, tengentDir.z, tangents[i].w);
            }
        }

        private static void RemapBones(BoneWeight[] boneWeights, System.ReadOnlySpan<int> boneIndices)
        {
            for (int i = 0; i < boneWeights.Length; i++)
            {
                if (boneWeights[i].weight0 > 0)
                {
                    boneWeights[i].boneIndex0 = boneIndices[boneWeights[i].boneIndex0];
                }
                if (boneWeights[i].weight1 > 0)
                {
                    boneWeights[i].boneIndex1 = boneIndices[boneWeights[i].boneIndex1];
                }
                if (boneWeights[i].weight2 > 0)
                {
                    boneWeights[i].boneIndex2 = boneIndices[boneWeights[i].boneIndex2];
                }
                if (boneWeights[i].weight3 > 0)
                {
                    boneWeights[i].boneIndex3 = boneIndices[boneWeights[i].boneIndex3];
                }
            }
        }

        private static bool CanReadMesh(Mesh mesh)
        {
#if UNITY_EDITOR
            return CanReadMeshInEditor(mesh);
#else
            return mesh.isReadable;
#endif
        }

#if UNITY_EDITOR
        private static System.Reflection.MethodInfo? meshCanAccessMethodInfo;

        // This is a workaround for a Unity peculiarity -
        // non-readable meshes are actually always accessible from the Editor.
        // We're still logging a warning since this won't work in a build.
        // ReSharper disable Unity.PerformanceAnalysis
        private static bool CanReadMeshInEditor(Mesh mesh)
        {
            if (mesh.isReadable)
                return true;
            else if (!Application.isPlaying)
                return true;

            if (meshCanAccessMethodInfo == null)
            {
                var canAccessProperty = typeof(Mesh).GetProperty("canAccess", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (canAccessProperty != null)
                    meshCanAccessMethodInfo = canAccessProperty.GetMethod;
            }

            if (meshCanAccessMethodInfo != null)
            {
                try
                {
                    bool canAccess = (bool)meshCanAccessMethodInfo.Invoke(mesh, null);
                    if (canAccess)
                    {
                        Debug.LogWarning("The mesh you are trying to access is not marked as readable. This will only work in the Editor and fail in a build.", mesh);
                        return true;
                    }
                }
                catch
                {
                    // There has probably been a Unity internal API update causing an error on this call.
                    return false;
                }
            }
            return false;
        }
#endif

        #endregion
    }
}
