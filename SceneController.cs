using Godot;
using OverwatchTranscript;
using OverwatchTranscriptViewer;
using System;
using System.Collections.Generic;

public class SceneController
{
	private static SceneController instance;
	private ITranscriptReader reader;
	private readonly Placer placer = new Placer();
	private readonly List<IScriptEventHandler> handlerRegistrations = new List<IScriptEventHandler>();
	
	public static SceneController Instance
	{
		get
		{
			if (instance == null) instance = new SceneController();
			return instance;
		}
	}

	public GuiController Gui;
	public Player Player;
	public event Action<AppState> AppStateChanged;

	public void RegisterScriptEventHandler(IScriptEventHandler handler)
	{
		handlerRegistrations.Add(handler);
	}
	
	public void LoadTranscript(string filepath)
	{
		AssertState(AppState.Empty);

		reader = Transcript.NewReader(filepath);

		foreach (var handler in handlerRegistrations)
		{
			handler.Initialize(reader, placer);
		}

		Player.Initialize(reader);

		SetState(AppState.Stopped);
	}

	public void PlayPause()
	{
		if (state == AppState.Stopped)
		{
			Player.Play();
			SetState(AppState.Playing);
		}
		else if (state == AppState.Playing)
		{
			Player.Stop();
			SetState(AppState.Stopped);
		}
		else
		{
			throw new Exception("Unexpected state: " + state);
		}
	}

	public void Proceed()
	{
		Player.Proceed();
	}

	public void Quit()
	{
		if (reader != null)
		{
			reader.Close();
		}
	}

	private AppState state;
	private void SetState(AppState state)
	{
		if (this.state == state) return;
		this.state = state;

		var handlers = AppStateChanged;
		if (handlers != null)
		{
			handlers(state);
		}
		GD.Print("State = " + state);
	}

	private void AssertState(AppState expected)
	{
		if (state != expected) throw new Exception("Unexpected state. Was: " + state + " expected: " + expected);
	}
}

public enum AppState
{
	Empty,
	Stopped,
	Playing,
	Jumping
}

public interface IScriptEventHandler
{
	void Initialize(ITranscriptReader reader, Placer placer);
}
