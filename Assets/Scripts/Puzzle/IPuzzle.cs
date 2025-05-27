///스크립트 생성 일자 - 2025 - 02 - 27
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;
using UnityEngine.Events;

namespace TempNamespace.Puzzles
{
	public interface IPuzzle
	{
		/// <summary>
		/// 퍼즐 시작 이벤트
		/// </summary>
		public UnityEvent OnPuzzleStart { get; }

		/// <summary>
		/// 퍼즐 시작
		/// </summary>
		public void PuzzleStart();
		
		/// <summary>
		/// 퍼즐 초기화 이벤트
		/// </summary>
		public UnityEvent OnPuzzleReset { get; }

		/// <summary>
		/// 퍼즐 초기화
		/// </summary>
		public void PuzzleReset();
		

		/// <summary>
		/// 퍼즐 클리어 이벤트
		/// </summary>
		public UnityEvent OnPuzzleClear { get; }

		/// <summary>
		/// 퍼즐 클리어
		/// </summary>
		public void PuzzleClear();
	}
}