///스크립트 생성 일자 - 2025 - 02 - 26
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TempNamespace.Utilities
{
    public static class TypeUtilities
    {     
        /// <summary>
        /// 특정 타입의 자식 클래스들을 리스트업
        /// </summary>
        /// <param name="includeAbstracts">추상 클래스 포함</param>
        /// <returns></returns>
        public static List<Type> GetAllTypes<T>(bool includeAbstracts = false)
        {
            Type classtype = typeof(T);

            var allTypes = classtype.Assembly.GetTypes();
            var SubTypeList = new List<Type>();

            for (int i = 0; i < allTypes.Length; i++)
            {
                if (allTypes[i].IsSubclassOf(classtype) && allTypes[i].IsAbstract == includeAbstracts)
                {
                    SubTypeList.Add(allTypes[i]);
                }
            }

            return SubTypeList;
        }
    }

    /// <summary>
    /// 물리적 / 수학적 계산을 보조하는 유틸리티 클래스
    /// </summary>
	public static class PhysicsUtilities
	{
		#region Validation Values
        /// <summary>
        /// float 값의 유효성 체크
        /// </summary>
        public static bool IsFinite(this float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
        /// <summary>
        /// Vector2 값의 유효성 체크
        /// </summary>
        public static bool IsFinite(this Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }
        /// <summary>
        /// Vector3 값의 유효성 체크
        /// </summary>
        public static bool IsFinite(this Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }
		#endregion
		
		#region Extends Calculation
        /// <summary>
        /// Angle 값을 min과 max 사이 값으로 조정
        /// </summary>
        public static float ClampAngle(float angle, float min, float max)
        {
            ///
            while (angle > max)
                angle -= 360.0f;
            while (angle < min)
                angle += 360.0f;
            ///
            return angle > max ? angle - (max + min) * 0.5f < 180.0f ? max : min : angle;
        }
        
        /// <summary>
        /// Angle 값을 0~360 사이 값으로 조정
        /// </summary>
        public static float ClampAngle(float angle)
        {
            angle = angle % 360.0f;

            if (angle < 0.0f)
            {
                angle += 360.0f;
            }

            return angle;
        }

        /// <summary>
        /// 즉시 int 제곱 얻기
        /// </summary>
		public static int Square(this int value)
		{
			return value*value;
		}
        /// <summary>
        /// 즉시 float 제곱 얻기
        /// </summary>
		public static float Square(this float value)
		{
			return value*value;
		}

        /// <summary>
        /// Vector3의 속도가 magnitude를 초과했는지 여부 확인(물리 오류 방지)
        /// </summary>
        public static bool IsExceeding(this Vector3 vector3, float magnitude)
        {
            const float kErrorTolerance = 1.01f;

            return vector3.sqrMagnitude > magnitude * magnitude * kErrorTolerance;
        }

        /// <summary>
        /// Vector2 값이 0인지 확인(물리 오류 방지)
        /// </summary>
		public static bool IsZero(this Vector2 self)
		{
			return self.sqrMagnitude < 9.99999943962493E-11;
		}
        /// <summary>
        /// Vector3 값이 0인지 확인(물리 오류 방지)
        /// </summary>
		public static bool IsZero(this Vector3 self)
		{
			return self.sqrMagnitude < 9.99999943962493E-11;
		}

        
        /// <summary>
        /// Vector3 내적 호출 단순화
        /// </summary>
		public static float Dot(this Vector3 self, Vector3 other)
		{
			return Vector3.Dot(self, other);
		}

        /// <summary>
        /// Vector3 외적 호출 단순화
        /// </summary>
		public static Vector3 Cross(this Vector3 self, Vector3 other)
		{
			return Vector3.Cross(self, other);
		}

        /// <summary>
        /// 바닥면에 수직인 법선 벡터 계산
        /// </summary>
		public static Vector3 ProjectOnPlane(this Vector3 self, Vector3 normal)
		{
			return Vector3.ProjectOnPlane(self, normal);
		}

		/// <summary>
		/// 직각 벡터의 법선 계산
		/// </summary>
		public static Vector3 PerpendicularTo(this Vector3 self, Vector3 other)
		{
			return Vector3.Cross(self, other).normalized;
		}

        /// <summary>
        /// 탄젠트 벡터 계산
        /// </summary>
		public static Vector3 TangentTo(this Vector3 self, Vector3 normal, Vector3 up)
		{
			Vector3 r = self.PerpendicularTo(up);
			Vector3 t = normal.PerpendicularTo(r);

			return t * self.magnitude;
		}
        public static Vector3 Lerp(this Vector3 self, Vector3 other, float t)
        {
            return Vector3.Lerp(self, other, t);
        }
        

        /// <summary>
        /// Transforms a vector to be relative to given transform.
        /// If isPlanar == true, the transform will be applied on the plane defined by upAxis.
        /// </summary>
        public static Vector3 RelativeTo(this Vector3 vector3, Transform relativeToThis, Vector3 upAxis, bool isPlanar = true)
        {
            Vector3 forward = relativeToThis.forward;

            if (isPlanar)
            {
                forward = Vector3.ProjectOnPlane(forward, upAxis);

                if (forward.IsZero())
                    forward = Vector3.ProjectOnPlane(relativeToThis.up, upAxis);
            }

            Quaternion q = Quaternion.LookRotation(forward, upAxis);

            return q * vector3;
        }
		#endregion

		#region MTD Calculation
		/****************************************
		두 개의 Collider 객체가 서로 떨어지기 위한 MTD(Minimal Translation Distance)를 계산
		****************************************/
		private const float MinMTDInflation = 0.0025f;
		private const float MaxMTDInflation = 0.0175f;
		public static bool ComputeMTD(CapsuleCollider colliderA, Vector3 positionA, Quaternion rotationA, 
									Collider colliderB, Vector3 positionB, Quaternion rotationB,
									out Vector3 mtdDirection, out float mtdDistance)
		{
			if(ComputeInflatedMTD(colliderA, positionA, rotationA, colliderB, positionB, rotationB,
				MinMTDInflation, out mtdDirection, out mtdDistance) ||
				ComputeInflatedMTD(colliderA, positionA, rotationA, colliderB, positionB, rotationB,
				MaxMTDInflation, out mtdDirection, out mtdDistance))
			{
				return true;
			}

			return false;
		}
		public static bool ComputeInflatedMTD(CapsuleCollider colliderA, Vector3 positionA, Quaternion rotationA,
									Collider colliderB, Vector3 positionB, Quaternion rotationB,
									float MTDInflation, out Vector3 mtdDirection, out float mtdDistance)
		{
			mtdDirection = Vector3.zero;
			mtdDistance = 0;

			colliderA.radius += MTDInflation;
			colliderA.height += MTDInflation*2;

			bool mtdResult = UnityEngine.Physics.ComputePenetration(colliderA, positionA, rotationA,
				colliderB, positionB, rotationB, out Vector3 recoverDirection, out float recoverDistance);

			if(mtdResult)
			{
				if(recoverDirection.IsFinite())
				{
					mtdDirection = recoverDirection;
					mtdDistance = Mathf.Max(Mathf.Abs(recoverDistance) - MTDInflation, 0) + 0.0001f;
				}
			}
			
			colliderA.radius -= MTDInflation;
			colliderA.height -= MTDInflation*2;

			return mtdResult;
		}
		
		#endregion
	}
	
    public static class MeshUtility
    {
        private const int MaxVertices = 1024;
        private const int MaxTriangles = MaxVertices * 3;

        private static readonly List<Vector3> _vertices = new List<Vector3>(MaxVertices);

        private static readonly List<ushort> _triangles16 = new List<ushort>(MaxTriangles);
        private static readonly List<int> _triangles32 = new List<int>();

        private static readonly List<ushort> _scratchBuffer16 = new List<ushort>(MaxTriangles);
        private static readonly List<int> _scratchBuffer32 = new List<int>();

        public static Vector3 FindMeshOpposingNormal(Mesh sharedMesh, ref RaycastHit inHit)
        {
            Vector3 v0, v1, v2;

            if (sharedMesh.indexFormat == IndexFormat.UInt16)
            {
                _triangles16.Clear();

                int subMeshCount = sharedMesh.subMeshCount;
                if (subMeshCount == 1)
                    sharedMesh.GetTriangles(_triangles16, 0);
                else
                {
                    for (int i = 0; i < subMeshCount; i++)
                    {
                        sharedMesh.GetTriangles(_scratchBuffer16, i);

                        _triangles16.AddRange(_scratchBuffer16);
                    }
                }
                
                sharedMesh.GetVertices(_vertices);

                v0 = _vertices[_triangles16[inHit.triangleIndex * 3 + 0]];
                v1 = _vertices[_triangles16[inHit.triangleIndex * 3 + 1]];
                v2 = _vertices[_triangles16[inHit.triangleIndex * 3 + 2]];
            }
            else
            {
                _triangles32.Clear();

                int subMeshCount = sharedMesh.subMeshCount;
                if (subMeshCount == 1)
                    sharedMesh.GetTriangles(_triangles32, 0);
                else
                {
                    for (int i = 0; i < subMeshCount; i++)
                    {
                        sharedMesh.GetTriangles(_scratchBuffer32, i);

                        _triangles32.AddRange(_scratchBuffer32);
                    }
                }
                
                sharedMesh.GetVertices(_vertices);

                v0 = _vertices[_triangles32[inHit.triangleIndex * 3 + 0]];
                v1 = _vertices[_triangles32[inHit.triangleIndex * 3 + 1]];
                v2 = _vertices[_triangles32[inHit.triangleIndex * 3 + 2]];
            }

            Matrix4x4 mtx = inHit.transform.localToWorldMatrix;

            Vector3 p0 = mtx.MultiplyPoint3x4(v0);
            Vector3 p1 = mtx.MultiplyPoint3x4(v1);
            Vector3 p2 = mtx.MultiplyPoint3x4(v2);

            Vector3 u = p1 - p0;
            Vector3 v = p2 - p0;

            Vector3 worldNormal = Vector3.Cross(u, v).normalized;
            
            if (Vector3.Dot(worldNormal, inHit.normal) < 0.0f)
                worldNormal = Vector3.Cross(v, u).normalized;

            return worldNormal;
        }

        public static void FlushBuffers()
        {
            _vertices.Clear();

            _scratchBuffer16.Clear();
            _scratchBuffer32.Clear();

            _triangles16.Clear();
            _triangles32.Clear();
        }
    }
}