using UnityEngine;
using TMPro;

namespace SPACE_WindowSystem
{
	public class TMP_InputFieldNoWrap : MonoBehaviour
	{
		[TextArea(minLines:2, maxLines:4)]
		[SerializeField] string README = @"attach this to TMP_inputField gameObject To Disable Wrapping";
		
		private TMP_InputField inputField;
		private TMP_Text textComponent;

		void Start()
		{
			inputField = GetComponent<TMP_InputField>();
			if (inputField != null)
			{
				textComponent = inputField.textComponent;

				// Force disable word wrapping
				DisableWordWrapping();

				// Subscribe to Channel (value change) to maintain the setting
				// disable wordWrapping every time a change is made
				inputField.onValueChanged.AddListener(OnTextChanged);
			}
		}

		void DisableWordWrapping()
		{
			if (textComponent != null)
			{
				textComponent.enableWordWrapping = false;
				textComponent.overflowMode = TextOverflowModes.Overflow;
			}
		}

		void OnTextChanged(string value)
		{
			// Ensure wrapping stays disabled even after text changes
			DisableWordWrapping();
		}

		void LateUpdate()
		{
			// Additional safety check in LateUpdate to prevent re-enabling
			if (textComponent != null && textComponent.enableWordWrapping)
			{
				DisableWordWrapping();
			}
		}

		void OnDestroy()
		{
			if (inputField != null)
				// unSubscribe from the Channel, when scene exit is made or gameObject is Destroyed
				inputField.onValueChanged.RemoveListener(OnTextChanged);
		}
	} 
}