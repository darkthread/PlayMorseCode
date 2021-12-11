
var wavPath = "test.wav";
MorseCodeModule.CreateWavFile(wavPath, args.Length == 0 ? "TEST" : args[0]);
System.Diagnostics.Process.Start("explorer", wavPath);
