using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;

class RawHID
{
    public const byte PROTOCOL_VERSION = 0x01;
    public const byte SUCCESS = 0x01;
    public const byte FAILED = 0xff;
    public static object StreamLock = new object();

    public int VendorId
    {
        get; private set;
    }
    public int ProductId
    {
        get; private set;
    }

    public enum Commands : byte
    {
        Protocol_GetVersion = 0x01,
        RGB_SetColorRGB = 0xC0,
        RGB_SetColorHSV = 0xC1,
        RGB_Step = 0xC2,
        RGB_Mode = 0xC3,
        RGB_Enable = 0xC4,
        RGB_Disable= 0xC5,
        Layer_Report = 0xB0,
        Layer_Invert = 0xB1,
        Layer_On = 0xB2,
        Layer_Off = 0xB3,
        Undefined = 0xff,
    };

    DeviceList list = DeviceList.Local;
    HidDevice hidDevice = null;
    HidStream hidStream = null;
    Thread readThread;

    public delegate void MessageReceivedCallback(byte[] Report);
    public event MessageReceivedCallback MessageReceived;

    public RawHID(int VendorId, int ProductId)
    {
        this.VendorId = VendorId;
        this.ProductId = ProductId;
    }

    public void Start()
    {
        this.hidDevice = FindDevice(this.VendorId, this.ProductId);
        DeviceList.Local.Changed += (object sender, DeviceListChangedEventArgs a) =>
        {
            if (this.hidDevice == null)
            {
                this.hidDevice = FindDevice(this.VendorId, this.ProductId);
            }
        };

        readThread = new Thread(() =>
        {
            while (true)
            {
                if (hidDevice == null)
                {
                    Console.WriteLine("No Numpad found, searching in background...");
                    while (hidDevice == null) Thread.Sleep(100);
                }

                if (hidDevice.TryOpen(out hidStream))
                {
                    hidStream.ReadTimeout = Timeout.Infinite;

                    while (true)
                    {
                        try
                        {
                            byte[] report = hidStream.Read();
                            this.RaiseEventOnUIThread(MessageReceived, new object[] { report });
                        }
                        catch (Exception e)
                        {
                            hidStream?.Close();
                            hidDevice = null;
                            hidStream = null;
                            break;
                        }

                    }
                }
                else
                {
                    Console.WriteLine("Error communicating with HID device.");
                    continue;
                }

                hidStream?.Close();
            }
        });
        readThread.Start();
    }

    public void Stop()
    {
        lock (StreamLock)
        {
            hidStream?.Close();
        }

        hidDevice = null;
        hidStream = null;
        readThread.Abort();
    }

    public void SendRaw(byte[] PayLoad)
    {
        lock (StreamLock)
        {
            hidStream.Write(PayLoad);
        }
    }

    public void SendRawPayload(Commands Command, byte[] PayLoad)
    {
        byte[] Message = BuildMessage(Command, PayLoad);
        lock (StreamLock)
        {
            if(hidStream != null)
                hidStream.Write(Message);
        }
    }

    public HidDevice FindDevice(int? VendorID, int? ProductID)
    {
        HidDevice hidDevice = null;
        foreach (var device in DeviceList.Local.GetHidDevices(VendorID, ProductID))
        {
            Console.WriteLine(device.DevicePath);
            HidStream hstrm = null;
            try
            {
                device.TryOpen(out hstrm);
                if (hstrm == null) continue;
                var GetProtocolRequest = BuildMessage(Commands.Protocol_GetVersion);

                hstrm.Write(GetProtocolRequest);

                byte[] response = hstrm.Read();


                if (response[1] == (byte)Commands.Protocol_GetVersion)
                {
                    if (response[2] != 0x01)
                    {

                        throw new Exception($"Expected Length is wrong: {response[1].ToString()}");
                    }
                    if (response[3] != PROTOCOL_VERSION)
                    {
                        throw new Exception($"Unsupported protocol version:{response[2].ToString()}");
                    }

                    Console.WriteLine("Negotiation successful.");
                    Console.WriteLine(device.GetManufacturer() + " " + device.GetProductName());
                    hidDevice = device;
                    break;
                }
                else
                {
                    throw new Exception($"Unsupported firmware:{response[0].ToString()}");
                }
            }
            catch (Exception e)
            {
                // Not a RawHID Device!!
                //Console.WriteLine(e.ToString());
            }
            finally
            {
                hstrm?.Close();
            }
        }

        return hidDevice;
    }

    private byte[] BuildMessage(Commands CommandID, byte[] payload = null)
    {
        byte length = payload != null ? (byte)payload.Length : (byte)0;
        byte[] rt = new byte[3+length];

        rt[0] = 0x00;
        rt[1] = (byte)CommandID;
        rt[2] = length;

        for(int i = 0; i< length; i++)
        {
            rt[i + 3] = payload[i];
        }
        
        return rt;
    }

    /// <summary>
    /// Fieser Hack!
    /// Checks the Target of each delegate in the event's invocation list, and marshal the call to the target thread if that target is ISynchronizeInvoke:
    /// </summary>
    /// <param name="theEvent"></param>
    /// <param name="args"></param>
    private void RaiseEventOnUIThread(Delegate theEvent, object[] args)
    {
        foreach (Delegate d in theEvent.GetInvocationList())
        {
            ISynchronizeInvoke syncer = d.Target as ISynchronizeInvoke;
            if (syncer == null)
            {
                d.DynamicInvoke(args);
            }
            else
            {
                syncer.BeginInvoke(d, args);
            }
        }
    }
}
