using Godot;
using OverwatchTranscript;
using OverwatchTranscriptViewer;
using System;
using System.Collections.Generic;
using System.Linq;

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

	public Player Player;
	public event Action<AppState> AppStateChanged;

	public void RegisterScriptEventHandler(IScriptEventHandler handler)
	{
		handlerRegistrations.Add(handler);
	}
	
	public void LoadTranscript(string filepath)
	{
		GD.Print("Opening: " + filepath);
		AssertState(AppState.Empty);

		if (!TryLoadFile(filepath)) return;

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

	public void Step()
	{
		Player.StepOne();
	}

	public void UpdatePlaybackSpeed(float speed)
	{
		AssertState(AppState.Empty, AppState.Stopped);
		Player.SetSpeed(speed);
	}

	public void AnimationBegin()
	{
		Player.AnimationBegin();
	}

	public void AnimationFinished()
	{
		Player.AnimationFinished();
	}

	public void Quit()
	{
		Player.Stop();
		if (reader != null)
		{
			reader.Close();
		}
	}

	private bool TryLoadFile(string filepath)
	{
		try
		{
			reader = Transcript.NewReader(filepath);
			return true;
		}
		catch (Exception ex)
		{
			reader = null;
			GD.Print("Failed to load file: " + ex);
			return false;
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

	private void AssertState(params AppState[] expected)
	{
		if (!expected.Contains(state)) throw new Exception("Unexpected state. Was: " + state + " expected: " + expected);
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
