using System;
using UnityEngine;

public class TooltipManager : MonoBehaviour {
	[SerializeField] private GameObject TooltipPrefab = null;
	[SerializeField] private RectTransform MainUICanvas = null;

	#region Singleton
	public static TooltipManager instance;
	private void Awake() {
		if (instance != null) {
			Debug.LogWarning("Multiple instances of TooltipManager detected.");
		}
		instance = this;
	}
	#endregion

	public Tooltip ShowTooltip(string tooltipText = "", float maxWidth = -1f, Func<Vector3> targetGetterFunction = null) {
		Tooltip tooltip = Instantiate(TooltipPrefab, MainUICanvas).GetComponent<Tooltip>();
		tooltip.Setup(MainUICanvas, tooltipText, maxWidth, targetGetterFunction);
		return tooltip;
	}
}
