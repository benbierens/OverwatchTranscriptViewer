using System;
using System.IO;

namespace OverwatchTranscript
{
	public static class Transcript
	{
		public static ITranscriptReader NewReader(string transcriptFile)
		{
			return new TranscriptReader(NewWorkDir(), transcriptFile);
		}

		private static string NewWorkDir()
		{
			return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		}
	}
}
