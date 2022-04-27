using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TextBufferCommon;
using TextBufferImplementations.ArrayBuffer;

namespace TextBufferTester
{
    [TestClass]
    public class TextBufferTests
    {
        private static readonly string FILENAME = "test.txt";
        private static string? bufferToUse;

        [TestInitialize]
        public void TestSetup()
        {
            // Read the buffer implementation to use for these unit tests. 
            // The tests are common for any implementation and ideally,
            // we want to test all of them. 
            var config = new ConfigurationBuilder()
                        .AddJsonFile("ut_configurations.json")
                        .Build();
            bufferToUse = config["BUFFER"];
        }

        [TestMethod]
        public void TextBuffer_TestFileLoad_BasicVerifications()
        {
            int numberOfLines = 10;

            var fileMock = TesterUtils.CreateFile(numberOfLines);
            var textBuffer = GetBufferImpl(fileMock.Object);
            var fileLines = fileMock.Object.ReadLines(FILENAME);

            // Load the contents of the file.
            textBuffer.LoadFile(FILENAME);

            // Verify that the contents of the file were successfully loaded.
            int lineNumber = 0, totalLength = 0;
            foreach (var line in fileLines)
            {
                // Match line by line
                textBuffer.GetLineContent(lineNumber).Should().Be(line);
                lineNumber++;
                totalLength += line.Length;
            }

            // Check the cursor position at the end of the file load.
            textBuffer.GetCursorPosition().Should().Be(totalLength);

            // Verify the total length tracked by the buffer matches what we expect.
            textBuffer.GetFileLength().Should().Be(totalLength);
        }

        [TestMethod]
        public void TextBuffer_Seek_VerifyCursorPositions()
        {
            var fileMock = TesterUtils.CreateFile();
            var textBuffer = GetBufferImpl(fileMock.Object);

            // Load the contents of the file.
            textBuffer.LoadFile(FILENAME);
            var totalFileSize = textBuffer.GetFileLength();

            textBuffer.Seek(totalFileSize - 5);
            textBuffer.GetCursorPosition().Should().Be(totalFileSize - 5);

            textBuffer.Seek(totalFileSize + 20);
            textBuffer.GetCursorPosition().Should().Be(totalFileSize);

            textBuffer.Seek(-5);
            textBuffer.GetCursorPosition().Should().Be(0);

            textBuffer.Seek(0);
            textBuffer.GetCursorPosition().Should().Be(0);

            textBuffer.Seek(totalFileSize);
            textBuffer.GetCursorPosition().Should().Be(totalFileSize);
        }

        [TestMethod]
        public void TextBuffer_Backspace_VerifyLineContentsAfterCharacterRemovals()
        {
            var fileLines = GetCustomFile();
            var fileMock = TesterUtils.CreateFileCustom(fileLines);
            var textBuffer = GetBufferImpl(fileMock.Object);
            var originalFileLength = textBuffer.GetFileLength();
            textBuffer.LoadFile(FILENAME);
            
            // The 35th character is the "f" in fence in the second line.
            textBuffer.Seek(35);

            // We remove 10 characters and now we get "jumped ence" in the second line.
            textBuffer.Backspace(10);
            textBuffer.GetLineContent(1).Should().Be("jumped ence");

            // This should take the cursor to the previous line i.e. line 1 and we will remove
            // 13 characters from there and will eventually get "The qu" as the first line.
            textBuffer.Backspace(20);
            textBuffer.GetLineContent(0).Should().Be("The qu");
            textBuffer.GetLineContent(1).Should().Be("ence");
            textBuffer.GetFileLength().Should().Be(originalFileLength - 30);

            // The 33rd character is "m" in the "random" on the third line.
            textBuffer.Seek(33);

            // This should ideally delete the line "and then did some random"
            textBuffer.Backspace(24);
            textBuffer.GetLineContent(2).Should().Be("stuff that we know nothing about");
            textBuffer.GetFileLength().Should().Be(originalFileLength - 54);

            // Take the cursor to the end of the file and empty out the entire file.s
            textBuffer.SeekToEnd();
            textBuffer.Backspace(textBuffer.GetFileLength());
            textBuffer.GetFileLength().Should().Be(0);
        }

        private IEnumerable<string> GetCustomFile()
        {
            return new List<string>()
            {
                "The quick brown fox",
                "jumped over the fence",
                "and then did some random",
                "stuff that we know nothing about",
                "but hey, Elon took over Twitter!",
            };
        }

        private ITextBuffer GetBufferImpl(FileWrapper file)
        {
            switch(bufferToUse)
            {
                case "ARRAY":
                    return new ArrayBuffer(file);
                default:
                    return new ArrayBuffer(file);
            }
        }
    }
}