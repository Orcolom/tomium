using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Tomia.Samples;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class Run
{
	[UnityTest]
	public IEnumerator RunWithEnumeratorPasses()
	{
		// EditorApplication.EnterPlaymode();

		SceneManager.LoadScene("Boot");
		yield return new WaitForSeconds(2);

		while (SampleRunner.HasDoneAllSamples() == false)
		{
			yield return null;
		}

		// EditorApplication.ExitPlaymode();
	}
}
