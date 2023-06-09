using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tomium.Samples
{
	public class SampleRunner : MonoBehaviour
	{
		private static SampleRunner _active;

		private static int _currentScene = 0;

		private void Awake()
		{
			_active = this;
			SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
			DontDestroyOnLoad(this);
		}

		private void Start()
		{
			NextSample();
		}

		private void SceneManagerOnSceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			Debug.LogWarning($"-- Loaded sample {arg0.name}");
		}

		public static void NextSample()
		{
			Debug.LogWarning($"-- Finished sample");
			if (_active == null) return;
			_currentScene++;
			if (_currentScene < SceneManager.sceneCountInBuildSettings) 
			SceneManager.LoadScene(_currentScene);
		}

		public static bool HasDoneAllSamples() => _currentScene >= SceneManager.sceneCountInBuildSettings;
	}
}
