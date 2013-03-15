﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class AdobeEncoreLineTabNewLine : SubtitleFormat
    {
        static Regex regexTimeCodes = new Regex(@"^\d\d\d\d \d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d\t", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Adobe Encore (line#/tabs/n)"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);
            if (sb.ToString().Contains(Environment.NewLine + "SP_NUMBER\tSTART\tEND\tFILE_NAME"))
                return false; // SON
            if (sb.ToString().Contains(Environment.NewLine + "SP_NUMBER     START        END       FILE_NAME"))
                return false; // SON

            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                //0002       00:01:48:22       00:01:52:17      - I need those samples, fast!
                //                                              - Yes, professor.
                string text = p.Text;
                text = text.Replace("<i>", "@Italic@");
                text = text.Replace("</i>", "@Italic@");
                text = Utilities.RemoveHtmlTags(text);
                if (Utilities.CountTagInText(Environment.NewLine, text) > 1)
                            text = Utilities.AutoBreakLineMoreThanTwoLines(text, Configuration.Settings.General.SubtitleLineMaximumLength);
                text = text.Replace(Environment.NewLine, Environment.NewLine + "\t\t\t\t");
                sb.AppendLine(string.Format("{0:0000} {1} {2}\t{3}", index, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text));
            }
            return sb.ToString();
        }

        private string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //0002       00:01:48:22       00:01:52:17      - I need those samples, fast!
            //                                              - Yes, professor.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line;
                if (regexTimeCodes.IsMatch(s))
                {
                    var temp = s.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (temp.Length > 1)
                    {
                        string start = temp[1];
                        string end = temp[2];

                        string[] startParts = start.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            try
                            {
                                string text = s.Remove(0, regexTimeCodes.Match(s).Length - 1).Trim();
                                if (!text.Contains(Environment.NewLine))
                                    text = text.Replace("//", Environment.NewLine);
                                if (text.Contains("@Italic@"))
                                {
                                    bool italicOn = false;
                                    while (text.Contains("@Italic@"))
                                    {
                                        int index = text.IndexOf("@Italic@");
                                        string italicTag = "<i>";
                                        if (italicOn)
                                            italicTag = "</i>";
                                        text = text.Remove(index, "@Italic@".Length).Insert(index, italicTag);
                                        italicOn = !italicOn;
                                    }
                                    text = Utilities.FixInvalidItalicTags(text);
                                }
                                p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), text);
                                subtitle.Paragraphs.Add(p);
                            }
                            catch (Exception exception)
                            {
                                _errorCount++;
                                System.Diagnostics.Debug.WriteLine(exception.Message);
                            }
                        }
                    }
                }
                else if (line == "\t\t\t" || line == "\t\t\t\t" || line == "\t\t\t\t\t")
                {
                    // skip empty lines
                }
                else if (line.StartsWith("\t\t\t\t") && p != null)
                {
                    if (p.Text.Length < 200)
                        p.Text = (p.Text + Environment.NewLine + line.Trim()).Trim();
                }
                else if (line.Trim().Length > 0 && p != null)
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
            return tc;
        }

    }
}
