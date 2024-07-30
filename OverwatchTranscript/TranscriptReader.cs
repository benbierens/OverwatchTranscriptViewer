using Newtonsoft.Json;
using System.IO;
using System;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;

namespace OverwatchTranscript
{
	public interface ITranscriptReader
	{
		OverwatchCommonHeader Header { get; }
		T GetHeader<T>(string key);
		void AddHandler<T>(Action<DateTime, T> handler);
		(DateTime, long)? Next();
		TimeSpan? GetDuration();
		void PreviewMoments(Func<OverwatchMoment, bool> previewer);
		void Close();
	}

	public class TranscriptReader : ITranscriptReader
	{
		private readonly string transcriptFile;
		private readonly string artifactsFolder;
		private readonly Dictionary<string, Action<DateTime, string>> handlers = new Dictionary<string, Action<DateTime, string>>();
		private readonly string workingDir;
		private OverwatchTranscript model = null!;
		private long momentIndex = 0;
		private bool closed;

		public TranscriptReader(string workingDir, string inputFilename)
		{
			closed = false;
			this.workingDir = workingDir;
			transcriptFile = Path.Combine(workingDir, TranscriptConstants.TranscriptFilename);
			artifactsFolder = Path.Combine(workingDir, TranscriptConstants.ArtifactFolderName);

			if (!Directory.Exists(workingDir)) Directory.CreateDirectory(workingDir);
			if (File.Exists(transcriptFile) || Directory.Exists(artifactsFolder)) throw new Exception("workingdir not clean");

			LoadModel(inputFilename);
		}

		public OverwatchCommonHeader Header
		{
			get
			{
				CheckClosed();
				return model.Header.Common;
			}
		}

		public T GetHeader<T>(string key)
		{
			CheckClosed();
			var value = model.Header.Entries.First(e => e.Key == key).Value;
			return JsonConvert.DeserializeObject<T>(value)!;
		}

		public void AddHandler<T>(Action<DateTime, T> handler)
		{
			CheckClosed();
			var typeName = typeof(T).FullName;
			if (string.IsNullOrEmpty(typeName)) throw new Exception("Empty typename for payload");

			handlers.Add(typeName, (utc, s) =>
			{
				handler(utc, JsonConvert.DeserializeObject<T>(s)!);
			});
		}

		/// <summary>
		/// Publishes the events at the next moment in time. Returns that moment.
		/// </summary>
		public (DateTime, long)? Next()
		{
			CheckClosed();
			if (momentIndex >= model.Moments.Length) return null;

			var moment = model.Moments[momentIndex];
			momentIndex++;

			PlayMoment(moment);
			return (moment.Utc, momentIndex);
		}

		/// <summary>
		/// Gets the time from the current moment to the next one.
		/// </summary>
		public TimeSpan? GetDuration()
		{
			if (momentIndex + 1 >= model.Moments.Length) return null;
			
			return
				model.Moments[momentIndex + 1].Utc -
				model.Moments[momentIndex].Utc;
		}

		public void PreviewMoments(Func<OverwatchMoment, bool> previewer)
		{
			foreach (var moment in model.Moments)
			{
				var proceed = previewer(moment);
				if (!proceed) return;
			}
		}

		public void Close()
		{
			CheckClosed();
			Directory.Delete(workingDir, true);
			closed = true;
		}

		private void PlayMoment(OverwatchMoment moment)
		{
			foreach (var @event in moment.Events)
			{
				PlayEvent(moment.Utc, @event);
			}
		}

		private void PlayEvent(DateTime utc, OverwatchEvent @event)
		{
			if (!handlers.ContainsKey(@event.Type)) return;
			var handler = handlers[@event.Type];

			handler(utc, @event.Payload);
		}

		private void LoadModel(string inputFilename)
		{
			ZipFile.ExtractToDirectory(inputFilename, workingDir);

			if (!File.Exists(transcriptFile))
			{
				closed = true;
				throw new Exception("Is not a transcript file. Unzipped to: " + workingDir);
			}

			model = JsonConvert.DeserializeObject<OverwatchTranscript>(File.ReadAllText(transcriptFile))!;
		}

		private void CheckClosed()
		{
			if (closed) throw new Exception("Transcript has already been closed.");
		}
	}
}
