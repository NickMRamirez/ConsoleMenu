﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using Moq.Sequences;
using NUnit.Framework;

namespace ConsoleMenu.Tests
{
    public class MenuTests
    {
        [Test]
        public void Constructor_GivenANullListOfMenuItems_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Menu("AnyString", null));
        }

        [Test]
        public void Display_FirstMenuItemIsDefault_SendsOneToInstructionPrintMethodAsDefaultValue()
        {
            var mockIoProvider = CreateMockIoProvider();
            var menuItems = new[] { new MenuItem('1', "default") { IsDefault = true }, new MenuItem('2', "non-default") };
            var menu = new Menu("SomeText", menuItems, mockIoProvider.Object);

            menu.Display();

            mockIoProvider.Verify(io => io.WriteInstructions(It.IsAny<string>(), 1));
        }

        [Test]
        public void Display_SecondMenuItemIsDefault_SendsTwoToInstructionPrintMethodAsDefaultValue()
        {
            var mockIoProvider = CreateMockIoProvider();
            var menuItems = new[] { new MenuItem('1', "non-default"), new MenuItem('2', "default") { IsDefault = true } };
            var menu = new Menu("SomeText", menuItems, mockIoProvider.Object);

            menu.Display();

            mockIoProvider.Verify(io => io.WriteInstructions(It.IsAny<string>(), 2));
        }

        [Test]
        public void Display_ThereAreNoMenuItems_Throws()
        {
            var mockIoProvider = CreateMockIoProvider();
            var menu = new Menu("SomeText", new IMenuItem[] { }, mockIoProvider.Object);

            Assert.Throws<InvalidOperationException>(() => menu.Display());
        }

        [Test]
        public void Display_InstructionPositionIsAbove_InstructionsAreWrittenBeforeChoices()
        {
            var mockIoProvider = CreateMockIoProvider();
            var menuItems = new[] { new MenuItem('1') };
            var menu = new Menu("SomeText", menuItems, mockIoProvider.Object) { InstructionPosition = InstructionPosition.Above };
            using (Sequence.Create())
            {
                mockIoProvider.Setup(io => io.WriteInstructions(It.IsAny<string>(), It.IsAny<int?>())).InSequence();
                mockIoProvider.Setup(io => io.WriteMenuItem(It.IsAny<char>(), It.IsAny<string>())).InSequence();

                menu.Display();
            }
        }

        [Test]
        public void Display_InstructionPositionIsBelow_InstructionsAreWrittenAfterChoices()
        {
            var mockIoProvider = CreateMockIoProvider();
            var menuItems = new[] { new MenuItem('1') };
            var menu = new Menu("SomeText", menuItems, mockIoProvider.Object) { InstructionPosition = InstructionPosition.Below };
            using (Sequence.Create())
            {
                mockIoProvider.Setup(io => io.WriteMenuItem(It.IsAny<char>(), It.IsAny<string>())).InSequence();
                mockIoProvider.Setup(io => io.WriteInstructions(It.IsAny<string>(), It.IsAny<int?>())).InSequence();

                menu.Display();
            }
        }

        [Test]
        public void Display_GivenDefaultValueAndEnterGivenAsInput_ReturnsTheMenuItemOfTheDefaultValue()
        {
            var mockIoProvider = CreateMockIoProvider();
            mockIoProvider.Setup(io => io.ReadCharacter()).Returns('\r');
            var defaultMenuItem = new MenuItem('2') { IsDefault = true };
            var menuItems = new[] { new MenuItem('1'), defaultMenuItem };
            var menu = new Menu("SomeText", menuItems, mockIoProvider.Object);

            var choice = menu.Display();

            Assert.AreSame(defaultMenuItem, choice);
        }

        [Test]
        public void Display_GivenMenuItemListWithMoreItemsThanWillFit_DoesNotDisplayItemsAfterLimit()
        {
            var mockIoProvider = CreateMockIoProvider();
            var menuItems = CreateMenuItemListWithMoreItemsThanWillFit();
            var menu = new Menu("SomeText", menuItems, mockIoProvider.Object);

            menu.Display();

            mockIoProvider.Verify(io => io.WriteMenuItem(It.Is<char>(value => value > '9'), It.IsAny<string>()), Times.Never);
        }

        private static IEnumerable<IMenuItem> CreateMenuItemListWithMoreItemsThanWillFit()
        {
            var menuItems = new List<IMenuItem>();
            for (int i = 0; i <= Menu.MaxOnScreen; i++)
            {
                menuItems.Add(new MenuItem(i.ToString(CultureInfo.InvariantCulture)[0]));
            }
            return menuItems;
        }

        private static Mock<IMenuIOProvider> CreateMockIoProvider()
        {
            var mockIoProvider = new Mock<IMenuIOProvider>();
            mockIoProvider.Setup(io => io.ReadCharacter()).Returns('1');
            return mockIoProvider;
        }
    }
}