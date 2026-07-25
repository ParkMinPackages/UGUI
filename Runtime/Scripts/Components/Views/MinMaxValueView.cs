using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace ParkMinPackages.UGUI.Components.Views
{
	public class MinMaxValueView : MonoBehaviour
	{
		// - Public Properties-
		[ShowInInspector, ReadOnly] public float Value
		{
			get { return _value; }
			set
			{
				if (Mathf.Approximately(_value, value)) return;
				UpdateValue(value);
				UpdateView();
			}
		}
		[ShowInInspector, ReadOnly] public int MinValue
		{
			get { return _minValue; }
			set
			{
				if (_minValue == value) return;
				UpdateMinValue(value);
				UpdateValue(_value);
				UpdateView();
			}
		}
		[ShowInInspector, ReadOnly] public int MaxValue
		{
			get { return _maxValue; }
			set
			{
				if (_maxValue == value) return;
				UpdateMaxValue(value);
				UpdateValue(_value);
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
		public InputField MinValueInputField
		{
			get { return _minValueInputField; }
		}
		public InputField MaxValueInputField
		{
			get { return _maxValueInputField; }
		}

		// - Internals -
		float _value = -1;
		int _minValue;
		int _maxValue = 1;
		[SerializeField, Required] Slider _valueSlider;
		[SerializeField, Required] Text _valueText;
		[SerializeField, Required] InputField _minValueInputField;
		[SerializeField, Required] InputField _maxValueInputField;

		protected virtual void Start() {
			if (int.TryParse(_maxValueInputField.text, out int maxValue)) UpdateMaxValue(maxValue);
			if (int.TryParse(_minValueInputField.text, out int minValue)) UpdateMinValue(minValue);
			UpdateValue(Mathf.Lerp(_minValue, _maxValue, _valueSlider.value));
			UpdateView();
			_minValueInputField.OnValueChangedAsObservable().Subscribe(value =>
			{
				if (int.TryParse(value, out int result)) MinValue = result;
			}).AddTo(gameObject);
			_maxValueInputField.OnValueChangedAsObservable().Subscribe(value =>
			{
				if (int.TryParse(value, out int result)) MaxValue = result;
			}).AddTo(gameObject);
			_valueSlider.OnValueChangedAsObservable().Subscribe(value =>
			{
				Value = Mathf.Lerp(_minValue, _maxValue, value);
			}).AddTo(gameObject);
		}
		void UpdateValue(float value) {
			_value = Mathf.Clamp(value, _minValue, _maxValue);
		}
		void UpdateMinValue(int value) {
			_minValue = Mathf.Clamp(value, int.MinValue, _maxValue);
		}
		void UpdateMaxValue(int value) {
			_maxValue = Mathf.Clamp(value, _minValue, int.MaxValue);
		}
		void UpdateView() {
			_valueText.text = _value.ToString("F1");
			_valueSlider.SetValueWithoutNotify(Mathf.InverseLerp(_minValue, _maxValue, _value));
			_minValueInputField.SetTextWithoutNotify(_minValue.ToString());
			_maxValueInputField.SetTextWithoutNotify(_maxValue.ToString());
		}
	}
}