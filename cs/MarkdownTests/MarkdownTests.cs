﻿using FluentAssertions;
using Markdown;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace MarkdownTests
{
    [TestFixture]
    public class MarkdownTests
    {
        private Md md;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            md = new Md();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Render_DoesNotThrow_OnNullOrWhitespace(string mdText)
        {
            Assert.DoesNotThrow(() => md.Render(mdText));
        }

        [TestCase(@"\_text\_", ExpectedResult = @"_text_")]
        [TestCase(@"\__text\__", ExpectedResult = @"_<em>text_<em/>")]
        [TestCase(@"\text text", ExpectedResult = @"\text text")]
        [TestCase(@"text \ text", ExpectedResult = @"text \ text")]
        [TestCase(@"text text \", ExpectedResult = @"text text \")]
        public string Render_ShouldCorrectRenderDisablingSymbol(string mdText)
        {
            return md.Render(mdText);
        }

        [TestCase("_italic_", ExpectedResult = "<em>italic<em/>", Description = "Italic style")]
        [TestCase("_ nonitalic_", ExpectedResult = "_ nonitalic_", Description = "Must be nonspace symbol after style begin token")]
        [TestCase("_nonitalic", ExpectedResult = "_nonitalic", Description = "Italic style begin token has no pair")]
        [TestCase("__bold__", ExpectedResult = "<strong>bold<strong/>", Description = "Bold style")]
        [TestCase("__ nonbold__", ExpectedResult = "<em>_ nonbold<em/>_", Description = "Must be nonspace symbol after style begin token")]
        [TestCase("__nonbold", ExpectedResult = "__nonbold", Description = "Bold style begin token has no pair")]
        public string Style_IsBeginingTokens_ReturnsCorrectValue(string text)
        {
            return md.Render(text);
        }

        [TestCase("_nonitalic _", ExpectedResult = "_nonitalic _", Description = "Must be nonspace symbol before style end token")]
        [TestCase("nonitalic_", ExpectedResult = "nonitalic_", Description = "Italic style end token has no pair")]
        [TestCase("__nonbold __", ExpectedResult = "<em>_nonbold _<em/>", Description = "Must be nonspace symbol before style end token")]
        [TestCase("nonbold__", ExpectedResult = "nonbold__", Description = "Bold style end token has no pair")]
        public string Style_IsEndingTokens_ReturnsCorrectValue(string text)
        {
            return md.Render(text);
        }

        [TestCase("_text0with0numbers _italic_", ExpectedResult = "_text0with0numbers <em>italic<em/>")]
        [TestCase("_italic text0with0numbers_ italic_", ExpectedResult = "<em>italic text0with0numbers_ italic<em/>")]
        [TestCase("text0_with_0numbers", ExpectedResult = "text0_with_0numbers")]
        public string Render_Should_IgnoreTagsInWordWithNumbers(string mdText)
        {
            return md.Render(mdText);
        }

        [TestCase("__bold _bolditalic_ bold__", ExpectedResult = "<strong>bold <em>bolditalic<em/> bold<strong/>")]
        [TestCase("_italic __nonbolditalic__ italic_", ExpectedResult = "<em>italic _<em/>nonbolditalic<em>_ italic<em/>")]
        public string Render_ShouldCorrectRender_WhenOneStyleIsIntoAnother(string mdText)
        {
            return md.Render(mdText);
        }

        [TestCase("_italicBegin _italicNotBegin italicEnd_ italicNotEnd_", ExpectedResult = "<em>italicBegin _italicNotBegin italicEnd<em/> italicNotEnd_")]
        [TestCase("__boldBegin __boldNotBegin boldEnd__ boldNotEnd__", ExpectedResult = "<strong>boldBegin __boldNotBegin boldEnd<strong/> boldNotEnd__")]
        public string Render_ShouldCorrectRender_WhenOneStyleIsIntoSameStyle(string mdText)
        {
            return md.Render(mdText);
        }

        [TestCase("__boldBegin _italicBegin boldEnd__ italicEnd_", ExpectedResult = "<strong>boldBegin _italicBegin boldEnd<strong/> italicEnd_")]
        public string Render_ShouldCorrectRender_WhenStylesBoundsAreIntersected(string mdText)
        {
            return md.Render(mdText);
        }

        [Test]
        public void Render_Duration_ShouldBeInLinearDependencyOfTextLength()
        {
            var testText = @"_Text_ _piece_ _for_ __Markdown__ _class_ _perfomance_ _test_. _Word_with_numbers_123_. \_Backslashed symbols\_. ";

            var factors = new List<double>();
            long previousDuration = 0;
            for (int i = 1; i <= 8; i++)
            {
                testText += testText; //test text length = 2^i
                var sw = Stopwatch.StartNew();
                md.Render(testText);
                sw.Stop();
                var factor = i > 1 ? (double)sw.ElapsedTicks / previousDuration : 0;
                previousDuration = sw.ElapsedTicks;
                factors.Add(factor);
            }
            factors.ForEach(durationFactor => durationFactor.Should().BeLessOrEqualTo(3.5, $"text length has been increased by 2"));
        }
    }
}
