using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Media {
    public sealed class Wav {
        int index;
        int dataSize;
        int sampleRate;

        /// <summary>
        /// Loads a WAV file. ONLY PCM 8-bit Mono.
        /// </summary>
        /// <param name="wav">WAV file bytes.</param>
        public Wav(byte[] wav) {
            // see https://ccrma.stanford.edu/courses/422/projects/WaveFormat/

            this.index = 0;
            if (wav[this.index + 0] != 'R' || wav[this.index + 1] != 'I' || wav[this.index + 2] != 'F' || wav[this.index + 3] != 'F') {
                throw new Exception("File is not RIFF");
            }
            this.index += 4;

            var chunkSize = BitConverter.ToUInt32(wav, this.index);
            // ChunkSize
            //uint ChunkSize = Utility.ExtractValueFromArray(wav, index, 4);
            this.index += 4;

            //format
            if (wav[this.index + 0] != 'W' || wav[this.index + 1] != 'A' || wav[this.index + 2] != 'V' || wav[this.index + 3] != 'E') {
                throw new Exception("File is not WAVE format");
            }
            this.index += 4;
            ////////////////////////////////////

            // fmt sub chunk //////////////////////
            //subchunk ID
            if (wav[this.index + 0] != 'f' || wav[this.index + 1] != 'm' || wav[this.index + 2] != 't' || wav[this.index + 3] != ' ') {
                throw new Exception("Unexpected fmt subchunk!");
            }
            this.index += 4;

            bool bitVarSampleRate16;

            var subchunk1Size = BitConverter.ToUInt32(wav, this.index); ;// Utility.ExtractValueFromArray(wav, index, 4);
            this.index += 4;
            if (subchunk1Size == 16) {
                bitVarSampleRate16 = true;
            }
            else if (subchunk1Size == 18) {
                bitVarSampleRate16 = false;
            }
            else {
                throw new Exception("Invalid Subchunk1Size.");
            }

            var audioFormat = BitConverter.ToUInt16(wav, this.index); ;// (ushort)Utility.ExtractValueFromArray(wav, index, 2);
            this.index += 2;
            if (audioFormat != 1) {
                throw new Exception("AudioFormat invalid.");
            }

            var numChannels = BitConverter.ToUInt16(wav, this.index);// (ushort)Utility.ExtractValueFromArray(wav, index, 2);
            this.index += 2;
            if (numChannels != 1) {
                throw new Exception("Must be mono.");
            }

            this.sampleRate = BitConverter.ToInt32(wav, this.index);// (int)Utility.ExtractValueFromArray(wav, index, 4);
            this.index += 4;
            if (this.sampleRate != 8000) {
                throw new Exception("Sample rate must be 8000KHz.");
            }

            var byteRate = BitConverter.ToUInt16(wav, this.index);// Utility.ExtractValueFromArray(wav, index, 4);
            this.index += 4;

            var blockAlign = BitConverter.ToUInt16(wav, this.index);// Utility.ExtractValueFromArray(wav, index, 2);
            this.index += 2;

            if (bitVarSampleRate16) {
                var bitsPerSample = BitConverter.ToUInt16(wav, this.index);// Utility.ExtractValueFromArray(wav, index, 2);
                this.index += 2;
                if (bitsPerSample != 8) {
                    throw new Exception("Must be 8 bit.");
                }
            }
            else {
                var bitsPerSample = BitConverter.ToUInt32(wav, this.index);// Utility.ExtractValueFromArray(wav, index, 4);
                this.index += 4;
                if (bitsPerSample != 8) {
                    throw new Exception("Must be 8 bit.");
                }
            }

            ///////////////////////////////////////////

            //// data sub-chunk ///////////////////////////////////////
            if (wav[this.index + 0] != 'd' || wav[this.index + 1] != 'a' || wav[this.index + 2] != 't' || wav[this.index + 3] != 'a') {
                throw new Exception("Unexpected data subchunk!");
            }
            this.index += 4;

            uint subchunk2Size = BitConverter.ToUInt16(wav, this.index);// Utility.ExtractValueFromArray(wav, index, 4);
            this.index += 4;

            this.dataSize = (int)subchunk2Size;
            ////////////////////////////////////////////
        }

        public int GetDataIndex() => this.index;

        public int GetDataSize() => this.dataSize;

        public int GetSampleRate() => this.sampleRate;
    }
}
