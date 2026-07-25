using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace ParkMinPackages.UGUI.Components.Views
{
	public class MaxValueView : MonoBehaviour
	{
		[ShowInInspector, ReadOnly] public float Value
		{
			get { return _value; }
			set
			{
				if (Mathf.Approximately(_value, value))
					return;


				UpdateValue(value);
				UpdateView();
			}
		}
		[ShowInInspector, ReadOnly] public int MaxValue
		{
			get { return _maxValue; }
			set
			{
				if (Mathf.Approximately(_maxValue, value))
					return;

				UpdateMaxValue(value);
				UpdateView();
			}
		}

		public Slider ValueSlider
		{
			get { return _valueSlider; }
		}
		public Text ValueText
		{
			get { return _valueText; }
		}
		public InputField MaxValueInputField
		{
			get { return _maxValueInputField; }
		}

		protected virtual void Start() {
			if (int.TryParse(_maxValueInputField.text, out int result)) {
				UpdateMaxValue(result);
			}
			UpdateValue(Mathf.Lerp(0, _maxValue, _valueSlider.value));
			UpdateView();

			_maxValueInputField.OnValueChangedAsObservable().Subscribe(f =>
			{
				if (int.TryParse(f, out int result)) {
					MaxValue = result;
				}
			}).AddTo(gameObject);
			_valueSlider.OnValueChangedAsObservable().Subscribe(f =>
			{
				Value = Mathf.Lerp(0, _maxValue, f);
			}).AddTo(gameObject);
		}

		float _value = -1;
		int _maxValue = -1;
		[SerializeField, Required] Slider _valueSlider;
		[SerializeField, Required] Text _valueText;
		[SerializeField, Required] InputField _maxValueInputField;

		void UpdateValue(float value) {
			_value = Mathf.Clamp(value, 0, _maxValue);
		}
		void UpdateMaxValue(int value) {
			_maxValue = Mathf.Clamp(value, 0, int.MaxValue);
		}
		void UpdateView() {
			_valueText.text = _value.ToString("F1");
			_valueSlider.SetValueWithoutNotify(_value / _maxValue);
			_maxValueInputField.SetTextWithoutNotify(_maxValue.ToString());
		}
	}
}