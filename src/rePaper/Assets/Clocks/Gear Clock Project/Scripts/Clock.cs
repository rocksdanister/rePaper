using System;
using UnityEngine;

public class Clock : MonoBehaviour {
	const float degreesPerHour = 30f, degreesPerMinute = 6f, degreesPerSecond = 6f;
	public Transform hoursTransform, minutesTransform, secondsTransform;
	public bool continuous;

	void Awake() {


		if (continuous) {
			TimeSpan time = DateTime.Now.TimeOfDay;

			hoursTransform.localRotation = Quaternion.Euler (0f, 0f, -1f * (float)time.TotalHours * degreesPerHour);
			minutesTransform.localRotation = Quaternion.Euler (0f,0f, -1f * (float)time.TotalMinutes * degreesPerMinute);
			secondsTransform.localRotation = Quaternion.Euler (0f, 0f,-1f* (float)time.TotalSeconds * degreesPerSecond);
		} else {
			DateTime time = DateTime.Now;

			hoursTransform.localRotation = Quaternion.Euler (0f, 0f, -1 * time.Hour * degreesPerHour); // Rotation storage. Multiply hour by 30 to get correct rotation around center
			minutesTransform.localRotation = Quaternion.Euler (0f, 0f, -1 * time.Minute * degreesPerMinute);
			secondsTransform.localRotation = Quaternion.Euler (0f, 0f, -1 * time.Second * degreesPerSecond);
		}
	}

	// Use this for initialization
	void Start() {
       // Application.targetFrameRate = 24;
	}

	// Update is called once per frame
	void Update() {
		Awake();
	}
}