using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogView : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI _text;
	void Awake()
	{
		Application.logMessageReceived += HandleLog;
	}

	List<Tuple<string, string>> errors = new List<Tuple<string, string>>();
	List<Tuple<string, string>> warnings = new List<Tuple<string, string>>();
	List<Tuple<string, string>> all = new List<Tuple<string, string>>();
	void HandleLog(string message, string stackTrace, LogType type)
	{
		if(_text != null)
			_text.text += message + "\n" + stackTrace + "\n";
		if (type == LogType.Exception)
		{
			warnings.Add(new Tuple<string, string>(message, stackTrace));
		}
		if (type == LogType.Error)
		{
			errors.Add(new Tuple<string, string>(message, stackTrace));
		}
		all.Add(new Tuple<string, string>(message,stackTrace));
		//_writer.WriteLine($"[{System.DateTime.Now:HH:mm:ss}] [{type}] {message}");
		//if (type == LogType.Exception || type == LogType.Error)
		//	_writer.WriteLine(stackTrace);
	}

	void OnDestroy()
	{
		Application.logMessageReceived -= HandleLog;
	}

	public void ToClipboard()
	{
		string finalText = "";
		foreach (var item in all)
			finalText += item.Item1 + "\n" + item.Item2 + "\n";
		GUIUtility.systemCopyBuffer = finalText;
	}

	public void WarningsToClipboard()
	{
		string finalText = "";
		foreach (var item in warnings)
			finalText += item.Item1 + "\n" + item.Item2 + "\n";
		GUIUtility.systemCopyBuffer = finalText;
	}

	public void ErrorsToClipboard()
	{
		string finalText = "";
		foreach (var item in errors)
			finalText += item.Item1 + "\n" + item.Item2 + "\n";
		GUIUtility.systemCopyBuffer = finalText;
	}
}