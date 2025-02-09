﻿using Ladeskab.Interfaces;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ladeskab.Test.Unit
{
    [TestFixture]
    public class TestStationControl
    {
        private StationControl _uut;
        private IDisplay _display;
        private IDoor _door;
        private IChargeControl _chargeControl;
        private IRfidReader _rfidReader;
        private ILogFile _logFile;

        [SetUp]
        public void SetUp()
        {
            _display = Substitute.For<IDisplay>();
            _door = Substitute.For<IDoor>();
            _chargeControl = Substitute.For<IChargeControl>();
            _rfidReader = Substitute.For<IRfidReader>();
            _uut = new StationControl(_door, _rfidReader, _display, _chargeControl, "testlog.txt");
        }

        [Test]
        public void Rfid_Available_Connected_DoorLocked()
        {
            _uut._state = StationControl.LadeskabState.Available;
            _chargeControl.IsConnected().Returns(true);
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _door.Received(1).LockDoor();
        }

        [Test]
        public void Rfid_Available_Connected_StartCharge()
        {
            _uut._state = StationControl.LadeskabState.Available;
            _chargeControl.IsConnected().Returns(true);
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _chargeControl.Received(1).StartCharge();
        }

        [Test]
        public void Rfid_Available_Connected_StateLocked()
        {
            _uut._state = StationControl.LadeskabState.Available;
            _chargeControl.IsConnected().Returns(true);
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            Assert.That(_uut._state, Is.EqualTo(StationControl.LadeskabState.Locked));
        }

        [Test]
        public void Rfid_Available_Connected_Display()
        {
            _uut._state = StationControl.LadeskabState.Available;
            _chargeControl.IsConnected().Returns(true);
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _display.Received(1).Print("Skabet er låst og din telefon lades. Brug dit RFID tag til at låse op.");
        }

        [Test]
        public void Rfid_Available_NotConnected_DoorSame()
        {
            _uut._state = StationControl.LadeskabState.Available;
            _chargeControl.IsConnected().Returns(false);
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _door.Received(0).LockDoor();
        }

        [Test]
        public void Rfid_Available_NotConnected_ChargeSame()
        {
            _uut._state = StationControl.LadeskabState.Available;
            _chargeControl.IsConnected().Returns(false);
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _chargeControl.Received(0).StartCharge();
        }

        [Test]
        public void Rfid_Available_NotConnected_StateSame()
        {
            _uut._state = StationControl.LadeskabState.Available;
            _chargeControl.IsConnected().Returns(false);
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            Assert.That(_uut._state, Is.EqualTo(StationControl.LadeskabState.Available));
        }

        [Test]
        public void Rfid_Available_NotConnected_Display()
        {
            _uut._state = StationControl.LadeskabState.Available;
            _chargeControl.IsConnected().Returns(false);
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _display.Received(1).Print("Din telefon er ikke ordentlig tilsluttet. Prøv igen.");
        }

        [Test]
        public void Rfid_DoorOpen_StartCharge()
        {
            _uut._state = StationControl.LadeskabState.DoorOpen;
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _chargeControl.DidNotReceive().StartCharge();
        }

        [Test]
        public void Rfid_DoorOpen_StopCharge()
        {
            _uut._state = StationControl.LadeskabState.DoorOpen;
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _chargeControl.DidNotReceive().StopCharge();
        }

        [Test]
        public void Rfid_Locked_CorrectRFID_DoorUnlocked()
        {
            _uut._state = StationControl.LadeskabState.Locked;
            _chargeControl.IsConnected().Returns(true);
            _uut._oldId = 3;
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _door.Received(1).UnlockDoor();
        }

        [Test]
        public void Rfid_Locked_CorrectRFID_StopCharge()
        {
            _uut._state = StationControl.LadeskabState.Locked;
            _chargeControl.IsConnected().Returns(true);
            _uut._oldId = 3;
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _chargeControl.Received(1).StopCharge();
        }

        [Test]
        public void Rfid_Locked_CorrectRFID_Display()
        {
            _uut._state = StationControl.LadeskabState.Locked;
            _chargeControl.IsConnected().Returns(true);
            _uut._oldId = 3;
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _display.Received(1).Print("Tag din telefon ud af skabet og luk døren");
        }

        [Test]
        public void Rfid_Locked_CorrectRFID_StateAvailable()
        {
            _uut._state = StationControl.LadeskabState.Locked;
            _chargeControl.IsConnected().Returns(true);
            _uut._oldId = 3;
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            Assert.That(_uut._state, Is.EqualTo(StationControl.LadeskabState.Available));
        }

        [Test]
        public void Rfid_Locked_WrongRFID_StopCharge()
        {
            _uut._state = StationControl.LadeskabState.Locked;
            _chargeControl.IsConnected().Returns(true);
            _uut._oldId = 2;
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _chargeControl.Received(0).StopCharge();
        }

        [Test]
        public void Rfid_Locked_WrongRFID_UnlockDoor()
        {
            _uut._state = StationControl.LadeskabState.Locked;
            _chargeControl.IsConnected().Returns(true);
            _uut._oldId = 2;
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _door.Received(0).UnlockDoor();
        }

        [Test]
        public void Rfid_Locked_WrongRFID_Display()
        {
            _uut._state = StationControl.LadeskabState.Locked;
            _chargeControl.IsConnected().Returns(true);
            _uut._oldId = 2;
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            _display.Received(1).Print("Forkert RFID tag");
        }

        [Test]
        public void Rfid_Locked_WrongRFID_StateSame()
        {
            _uut._state = StationControl.LadeskabState.Locked;
            _chargeControl.IsConnected().Returns(true);
            _uut._oldId = 2;
            _rfidReader.rfidEvent += Raise.EventWith(new rfidEventArgs() { id = 3 });
            Assert.That(_uut._state, Is.EqualTo(StationControl.LadeskabState.Locked));
        }

        [Test]
        public void DoorEvent_Available_DoorOpenTrue_Display()
        {
            _uut._state = StationControl.LadeskabState.Available;
            _door.DoorEvent += Raise.EventWith(new DoorEventArgs() { DoorOpen = true });
            _display.Received(1).Print("Tilslut telefon");
        }

        [Test]
        public void DoorEvent_Available_DoorOpenTrue_StateOpen()
        {
            _uut._state = StationControl.LadeskabState.Available;
            _door.DoorEvent += Raise.EventWith(new DoorEventArgs() { DoorOpen = true });
            Assert.That(_uut._state, Is.EqualTo(StationControl.LadeskabState.DoorOpen));
        }

        [Test]
        public void DoorEvent_Open_DoorOpenFalse_Display()
        {
            _uut._state = StationControl.LadeskabState.DoorOpen;
            _door.DoorEvent += Raise.EventWith(new DoorEventArgs() { DoorOpen = false });
            _display.Received(1).Print("Indlæs RFID");
        }

        [Test]
        public void DoorEvent_Open_DoorEventFalse_StateAvailable()
        {
            _uut._state = StationControl.LadeskabState.DoorOpen;
            _door.DoorEvent += Raise.EventWith(new DoorEventArgs() { DoorOpen = false });
            Assert.That(_uut._state, Is.EqualTo(StationControl.LadeskabState.Available));
        }
    }
}

