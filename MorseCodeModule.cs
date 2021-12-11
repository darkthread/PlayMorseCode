public class MorseCodeModule
{
    //REF: https://github.com/cduck/morse
    static Dictionary<char, string> MorseCodeTable = new Dictionary<char, string>
    {
        ['A'] = ".-",
        ['B'] = "-...",
        ['C'] = "-.-.",
        ['D'] = "-..",
        ['E'] = ".",
        ['F'] = "..-.",
        ['G'] = "--.",
        ['H'] = "....",
        ['I'] = "..",
        ['J'] = ".---",
        ['K'] = "-.-",
        ['L'] = ".-..",
        ['M'] = "--",
        ['N'] = "-.",
        ['O'] = "---",
        ['P'] = ".--.",
        ['Q'] = "--.-",
        ['R'] = ".-.",
        ['S'] = "...",
        ['T'] = "-",
        ['U'] = "..-",
        ['V'] = "...-",
        ['W'] = ".--",
        ['X'] = "-..-",
        ['Y'] = "-.--",
        ['Z'] = "--..",
        ['0'] = "-----",
        ['1'] = ".----",
        ['2'] = "..---",
        ['3'] = "...--",
        ['4'] = "....-",
        ['5'] = ".....",
        ['6'] = "-....",
        ['7'] = "--...",
        ['8'] = "---..",
        ['9'] = "----.",
        ['.'] = ".-.-.-",
        [','] = "--..--",
        ['?'] = "..--..",
        ['\\'] = ".----.",
        ['!'] = "-.-.--",
        ['/'] = "-..-.",
        ['('] = "-.--.",
        [')'] = "-.--.-",
        ['&'] = ".-...",
        [':'] = "---...",
        [';'] = "-.-.-.",
        ['='] = "-...-",
        ['+'] = ".-.-.",
        ['-'] = "-....-",
        ['_'] = "..--.-",
        ['\"'] = ".-..-.",
        ['$'] = "...-..-",
        ['@'] = ".--.-.",
        ['\x02'] = "-.-.-", //Start
        ['\x03'] = "...-." //End
    };


    static string TextToMorseCode(string message)
    {
        return string.Join("000", $"\x2 {message} \x3".ToUpper().ToArray()
            .Select(ch =>
            {
                if (ch == ' ') return "0000"; //Word Spacing 3+4
                return string.Join("0",
                    MorseCodeTable[ch].ToArray()
                    .Select(o => o == '.' ? "1" : "111")
                    .ToArray());
            }).ToArray());
    }

    public static void CreateWavFile(string wavPath, string message)
    {
        var f = new FileStream(wavPath, FileMode.Create);
        CreateWav(f, " " + message);
        f.Dispose();
    }

    public static void CreateWav(Stream f, string message)
    {
        ushort channelCount = 1;
        ushort sampleBytes = 1; // in bytes
        uint sampleRate = 8000;
        int freq = 641; // US Army 641
        int dotPerSec = 20;

        var bitData = TextToMorseCode(message);

        // 641Hz tone sample
        byte[] toneSample = new byte[sampleRate / freq];
        for (var i = 0; i < toneSample.Length; i++)
        {
            byte v = i > toneSample.Length / 2 ? (byte)255 : (byte)0;
            toneSample[i] = v;
        }

        uint dataLen = (uint)(sampleRate / dotPerSec * bitData.Length);

        var wr = new BinaryWriter(f);
        wr.Write("RIFF".ToArray());
        uint fileLength = 36 + dataLen * channelCount * sampleBytes;
        wr.Write(fileLength); //FileLength
        wr.Write("WAVEfmt ".ToArray());
        wr.Write(16); //ChunkSize
        wr.Write((ushort)1); //FormatTag
        wr.Write(channelCount); //Channels
        wr.Write(sampleRate); //Frequency
        wr.Write(sampleRate * sampleBytes * channelCount); //AverageBytesPerSec
        wr.Write((ushort)(sampleBytes * channelCount)); //BlockAlign
        wr.Write((ushort)(8 * sampleBytes)); //BitsPerSample
        wr.Write("data".ToArray());
        wr.Write(dataLen * sampleBytes); //ChunkSize

        var sampleIdx = 0;
        var bitIndex = 0;
        var mute = false;
        int dotDura = 0;
        for (int i = 0; i < dataLen; i++)
        {
            dotDura--;
            if (dotDura <= 0)
            {
                dotDura = (int)sampleRate / dotPerSec;
                mute = bitData[bitIndex] == '0';
                bitIndex++;
            }
            wr.Write(mute ? (byte)127 : toneSample[sampleIdx]);
            sampleIdx++;
            if (sampleIdx >= toneSample.Length) sampleIdx = 0;
        }
        wr.Dispose();
    }
}