// <copyright file="TextBufferTester.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

namespace TextBufferTester
{
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using TextBufferCommon;
    using TextBufferImplementations.ArrayBuffer;

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
            IList<string> fileLines = new List<string>();
            var fileMock = TesterUtils.CreateFile(ref fileLines, numberOfLines);
            var textBuffer = GetBufferImpl(fileMock.Object);

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
            IList<string> fileLines = new List<string>();
            var fileMock = TesterUtils.CreateFile(ref fileLines);
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
            
            // The 36th character is the "f" in fence in the second line.
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

            // Take the cursor to the end of the file and empty out the entire file
            textBuffer.SeekToEnd();
            textBuffer.Backspace(textBuffer.GetFileLength());
            textBuffer.GetFileLength().Should().Be(0);
        }

        [TestMethod]
        public void TextBuffer_Delete_VerifyLineContentsAfterCharacterRemovals()
        {
            var fileLines = GetCustomFile();
            var fileMock = TesterUtils.CreateFileCustom(fileLines);
            var textBuffer = GetBufferImpl(fileMock.Object);
            var originalFileLength = textBuffer.GetFileLength();
            textBuffer.LoadFile(FILENAME);

            // The 8th character is the "c" in "quick" in the first line.
            textBuffer.Seek(7);

            // We remove 10 characters and now we get "The quiox" in first line.
            textBuffer.Delete(10);
            textBuffer.GetLineContent(0).Should().Be("The quiox");

            // This should take the cursor to the next line i.e. line 1 and we will remove
            // 11 characters from there.
            textBuffer.Delete(13);
            textBuffer.GetLineContent(0).Should().Be("The qui");
            textBuffer.GetLineContent(1).Should().Be(" the fence");
            textBuffer.GetFileLength().Should().Be(originalFileLength - 23);

            // This should ideally delete the line " the fence"
            textBuffer.Delete(10);
            textBuffer.GetLineContent(1).Should().Be("and then did some random");
            textBuffer.GetFileLength().Should().Be(originalFileLength - 33);

            // Take the cursor to the beginning of the file and empty out the entire file
            textBuffer.SeekToBegin();
            textBuffer.Delete(textBuffer.GetFileLength());
            textBuffer.GetFileLength().Should().Be(0);
        }

        [TestMethod]
        public void TextBuffer_Insert_EmptyFile_VerifyLineContent()
        {
            var fileLines = new List<string>();
            var fileMock = TesterUtils.CreateFileCustom(fileLines);
            var textBuffer = GetBufferImpl(fileMock.Object);
            var originalFileLength = textBuffer.GetFileLength();
            textBuffer.LoadFile(FILENAME);

            var stringToInsert = "pop pop, from the community";

            // File should be empty initially.
            originalFileLength.Should().Be(0);

            textBuffer.Insert(stringToInsert);
            textBuffer.GetLineContent(0).Should().Be("pop pop, from the community");
            textBuffer.GetFileLength().Should().Be(stringToInsert.Length);
            textBuffer.GetCursorPosition().Should().Be(stringToInsert.Length);
        }


        [TestMethod]
        public void TextBuffer_Insert_NonEmptyFile_VerifyLineContent()
        {
            var fileLines = GetCustomFile();
            var fileMock = TesterUtils.CreateFileCustom(fileLines);
            var textBuffer = GetBufferImpl(fileMock.Object);
            var originalFileLength = textBuffer.GetFileLength();
            textBuffer.LoadFile(FILENAME);

            var stringToInsert = "pop pop, from the community";

            // The 8th character is the "c" in "quick" in the first line.
            textBuffer.Seek(7);

            textBuffer.Insert(stringToInsert);
            textBuffer.GetLineContent(0).Should().Be("The quicpop pop, from the communityk brown fox");
            textBuffer.GetFileLength().Should().Be(originalFileLength + stringToInsert.Length);
        }

        [TestMethod]
        public void TextBuffer_Undo_VerifyVariousOperations()
        {
            var fileLines = GetCustomFile();
            var fileMock = TesterUtils.CreateFileCustom(fileLines);
            var textBuffer = GetBufferImpl(fileMock.Object);
            textBuffer.LoadFile(FILENAME);

            var originalFileLength = textBuffer.GetFileLength();

            // Brings us to the "d" of the "and" of the third line.
            textBuffer.Seek(42);

            // Insert a new text and verify.
            textBuffer.Insert(" random ");
            textBuffer.GetLineContent(2).Should().Be("and random then did some random");

            textBuffer.Undo();
            textBuffer.GetLineContent(2).Should().Be("and then did some random");

            textBuffer.Delete(82);
            textBuffer.GetLineContent(3).Should().Be("er!");

            textBuffer.Undo();
            textBuffer.GetLineContent(3).Should().Be("stuff that we know nothing about");

            textBuffer.SeekToEnd();
            textBuffer.Backspace(textBuffer.GetFileLength());
            textBuffer.Undo();
            textBuffer.GetFileLength().Should().Be(originalFileLength);
            textBuffer.GetCursorPosition().Should().Be(originalFileLength);
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

        private ITextBuffer GetBufferImpl(IFile file)
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