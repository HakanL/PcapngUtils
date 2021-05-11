using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haukcode.PcapngUtils.Common
{
    public enum LinkTypes : ushort
    {
        /// <summary>
        /// No link layer information. A packet saved with this link layer contains a raw L3 packet preceded by a 32-bit host-byte-order AF_ value indicating the specific L3 type.
        /// </summary>
        Null = 0,

        /// <summary>
        /// D/I/X and 802.3 Ethernet
        /// </summary>
        Ethernet = 1,

        /// <summary>
        /// Experimental Ethernet (3Mb)
        /// </summary>
        ExpEthernet = 2,

        /// <summary>
        /// Amateur Radio AX.25
        /// </summary>
        AX25 = 3,

        /// <summary>
        /// Proteon ProNET Token Ring
        /// </summary>
        ProNET = 4,

        /// <summary>
        /// Chaos
        /// </summary>
        Chaos = 5,

        /// <summary>
        /// IEEE 802 Networks
        /// </summary>
        TokenRing = 6,

        /// <summary>
        /// ARCNET, with BSD-style header
        /// </summary>
        ARCNET = 7,

        /// <summary>
        /// Serial Line IP
        /// </summary>
        SLIP = 8,

        /// <summary>
        /// Point-to-point Protocol
        /// </summary>
        PPP = 9,

        /// <summary>
        /// FDDI
        /// </summary>
        FDDI = 10,

        /// <summary>
        /// PPP in HDLC-like framing
        /// </summary>
        PppHdlc = 50,

        /// <summary>
        /// NetBSD PPP-over-Ethernet
        /// </summary>
        PppEthernet = 51,

        /// <summary>
        /// Symantec Enterprise Firewall
        /// </summary>
        SymantecFirewall = 99,

        /// <summary>
        /// LLC/SNAP-encapsulated ATM
        /// </summary>
        AtmRfc1483 = 100,

        /// <summary>
        /// Raw IP
        /// </summary>
        Raw = 101,

        /// <summary>
        /// BSD/OS SLIP BPF header
        /// </summary>
        SlipBsdos = 102,

        /// <summary>
        /// BSD/OS PPP BPF header
        /// </summary>
        PppBsdos = 103,

        /// <summary>
        /// Cisco HDLC
        /// </summary>
        CiscoHdlc = 104,

        /// <summary>
        /// IEEE 802.11 (wireless)
        /// </summary>
        Ieee80211 = 105,

        /// <summary>
        /// Linux Classical IP over ATM
        /// </summary>
        AtmClip = 106,

        /// <summary>
        /// Frame Relay
        /// </summary>
        FrameRelay = 107,

        /// <summary>
        /// OpenBSD loopback
        /// </summary>
        Loop = 108,

        /// <summary>
        /// OpenBSD IPSEC enc
        /// </summary>
        ENC = 109,

        /// <summary>
        /// ATM LANE + 802.3 (Reserved for future use)
        /// </summary>
        Lane8023 = 110,

        /// <summary>
        /// NetBSD HIPPI (Reserved for future use)
        /// </summary>
        HIPPI = 111,

        /// <summary>
        /// NetBSD HDLC framing (Reserved for future use)
        /// </summary>
        HDLC = 112,

        /// <summary>
        /// Linux cooked socket capture
        /// </summary>
        LinuxSll = 113,

        /// <summary>
        /// Apple LocalTalk hardware
        /// </summary>
        LocalTalk = 114,

        /// <summary>
        /// Acorn Econet
        /// </summary>
        AcornEconet = 115,

        /// <summary>
        /// Reserved for use with OpenBSD ipfilter
        /// </summary>
        IpFilter = 116,

        /// <summary>
        /// OpenBSD DLT_PFLOG
        /// </summary>
        PfLog = 117,

        /// <summary>
        /// For Cisco-internal use
        /// </summary>
        CiscoIos = 118,

        /// <summary>
        /// 802.11+Prism II monitor mode
        /// </summary>
        PrismHeader = 119,

        /// <summary>
        /// FreeBSD Aironet driver stuff
        /// </summary>
        AironetHeader = 120,

        /// <summary>
        /// Reserved for Siemens HiPath HDLC
        /// </summary>
        HHDLC = 121,

        /// <summary>
        /// RFC 2625 IP-over-Fibre Channel
        /// </summary>
        IpOverFibre = 122,

        /// <summary>
        /// Solaris+SunATM
        /// </summary>
        SunAtm = 123,

        /// <summary>
        /// RapidIO - Reserved as per request from Kent Dahlgren <kent@praesum.com> for private use.
        /// </summary>
        RapidIo = 124,

        /// <summary>
        /// PCI Express - Reserved as per request from Kent Dahlgren <kent@praesum.com> for private use.
        /// </summary>
        PciExpress = 125,

        /// <summary>
        /// Xilinx Aurora link layer - Reserved as per request from Kent Dahlgren <kent@praesum.com> for private use.
        /// </summary>
        Aurora = 126,

        /// <summary>
        /// 802.11 plus BSD radio header
        /// </summary>
        Ieee80211Radio = 127,

        /// <summary>
        /// Tazmen Sniffer Protocol - Reserved for the TZSP encapsulation, as per request from Chris Waters <chris.waters@networkchemistry.com> TZSP is a generic encapsulation for any other link type, which includes a means to include meta-information with the packet, e.g. signal strength and channel for 802.11 packets.
        /// </summary>
        TZSP = 128,

        /// <summary>
        /// Linux-style headers
        /// </summary>
        ArcnetLinux = 129,

        /// <summary>
        /// Juniper-private data link type, as per request from Hannes Gredler <hannes@juniper.net>. The corresponding DLT_s are used for passing on chassis-internal metainformation such as QOS profiles, etc..
        /// </summary>
        JuniperMlPpp = 130,

        /// <summary>
        /// Juniper-private data link type, as per request from Hannes Gredler <hannes@juniper.net>. The corresponding DLT_s are used for passing on chassis-internal metainformation such as QOS profiles, etc..
        /// </summary>
        JuniperMlfr = 131,

        /// <summary>
        /// Juniper-private data link type, as per request from Hannes Gredler <hannes@juniper.net>. The corresponding DLT_s are used for passing on chassis-internal metainformation such as QOS profiles, etc..
        /// </summary>
        JuniperES = 132,

        /// <summary>
        /// Juniper-private data link type, as per request from Hannes Gredler <hannes@juniper.net>. The corresponding DLT_s are used for passing on chassis-internal metainformation such as QOS profiles, etc..
        /// </summary>
        JuniperGgsn = 133,

        /// <summary>
        /// Juniper-private data link type, as per request from Hannes Gredler <hannes@juniper.net>. The corresponding DLT_s are used for passing on chassis-internal metainformation such as QOS profiles, etc..
        /// </summary>
        JuniperMfr = 134,

        /// <summary>
        /// Juniper-private data link type, as per request from Hannes Gredler <hannes@juniper.net>. The corresponding DLT_s are used for passing on chassis-internal metainformation such as QOS profiles, etc..
        /// </summary>
        JuniperAtm2 = 135,

        /// <summary>
        /// Juniper-private data link type, as per request from Hannes Gredler <hannes@juniper.net>. The corresponding DLT_s are used for passing on chassis-internal metainformation such as QOS profiles, etc..
        /// </summary>
        JuniperServices = 136,

        /// <summary>
        /// Juniper-private data link type, as per request from Hannes Gredler <hannes@juniper.net>. The corresponding DLT_s are used for passing on chassis-internal metainformation such as QOS profiles, etc..
        /// </summary>
        JuniperAtm1 = 137,

        /// <summary>
        /// Apple IP-over-IEEE 1394 cooked header
        /// </summary>
        AppleIpOverIeee1394 = 138,

        /// <summary>
        /// ???
        /// </summary>
        Mtp2WithPhdr = 139,

        /// <summary>
        /// ???
        /// </summary>
        Mtp2 = 140,

        /// <summary>
        /// ???
        /// </summary>
        Mtp3 = 141,

        /// <summary>
        /// Signalling Connection Control Part (SCCP)
        /// </summary>
        SCCP = 142,

        /// <summary>
        /// DOCSIS MAC frames
        /// </summary>
        DOCSIS = 143,

        /// <summary>
        /// Linux-IrDA
        /// </summary>
        LinuxIrDA = 144,

        /// <summary>
        /// Reserved for IBM SP switch and IBM Next Federation switch.
        /// </summary>
        IbmSP = 145,

        /// <summary>
        /// Reserved for IBM SP switch and IBM Next Federation switch.
        /// </summary>
        IbmSN = 146,

        /// <summary>
        /// Reserved for private use
        /// </summary>
        User0 = 147,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User1 = 148,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User2 = 149,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User3 = 150,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User4 = 151,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User5 = 152,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User6 = 153,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User7 = 154,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User8 = 155,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User9 = 156,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User10 = 157,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User11 = 158,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User12 = 159,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User13 = 160,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User14 = 161,
        /// <summary>
        /// Reserved for private use
        /// </summary>
        User15 = 162,
        /// <summary>
        /// AVS monitor mode information followed by an 802.11 header.
        /// </summary>
        Ieee80211Avs = 163,

        /// <summary>
        /// ???
        /// </summary>
        JuniperMonitor = 164,

        /// <summary>
        /// BACnet MS/TP frames, as specified by section 9.3 MS/TP Frame Format of ANSI/ASHRAE Standard 135, BACnet® - A Data Communication Protocol for Building Automation and Control Networks, including the preamble and, if present, the Data CRC.
        /// </summary>
        BacnetMsTp = 165,

        /// <summary>
        /// PPP in HDLC-like encapsulation, like LINKTYPE_PPP_HDLC, but with the 0xff address byte replaced by a direction indication - 0x00 for incoming and 0x01 for outgoing.
        /// </summary>
        PppPppd = 166,

        /// <summary>
        /// ???
        /// </summary>
        JuniperPppoe = 167,

        /// <summary>
        /// ???
        /// </summary>
        JuniperPppoeAtm = 168,

        /// <summary>
        /// General Packet Radio Service Logical Link Control, as defined by 3GPP TS 04.64.
        /// </summary>
        GprsLlc = 169,

        /// <summary>
        /// Transparent-mapped generic framing procedure, as specified by ITU-T Recommendation G.7041/Y.1303.
        /// </summary>
        GpfT = 170,

        /// <summary>
        /// Frame-mapped generic framing procedure, as specified by ITU-T Recommendation G.7041/Y.1303.
        /// </summary>
        GpfF = 171,

        /// <summary>
        /// ???
        /// </summary>
        GcomTie1 = 172,

        /// <summary>
        /// ???
        /// </summary>
        GcomSerial = 173,
        
        /// <summary>
        /// ???
        /// </summary>
        JuniperPicPeer = 174,

        /// <summary>
        /// ???
        /// </summary>
        ErfEth = 175,

        /// <summary>
        /// ???
        /// </summary>
        ErfPos = 176,

        /// <summary>
        /// Link Access Procedures on the D Channel (LAPD) frames, as specified by ITU-T Recommendation Q.920 and ITU-T Recommendation Q.921, captured via vISDN, with a LINKTYPE_LINUX_LAPD header, followed by the Q.921 frame, starting with the address field.
        /// </summary>
        LinuxLapd = 177,

        /// <summary>
        /// ???
        /// </summary>
        JuniperEther = 178,

        /// <summary>
        /// ???
        /// </summary>
        JuniperPpp = 179,
        
        /// <summary>
        /// ???
        /// </summary>
        JuniperFrelay = 180,

        /// <summary>
        /// ???
        /// </summary>
        JuniperChdlc = 181,

        /// <summary>
        /// FRF.16.1 Multi-Link Frame Relay frames, beginning with an FRF.12 Interface fragmentation format fragmentation header.
        /// </summary>
        Mfr = 182,

        /// <summary>
        /// ???
        /// </summary>
        JuniperVp = 183,

        /// <summary>
        /// ???
        /// </summary>
        A429 = 184,

        /// <summary>
        /// ???
        /// </summary>
        A653Icm = 185,

        /// <summary>
        /// ???
        /// </summary>
        Usb = 186,

        /// <summary>
        /// ???
        /// </summary>
        BluetoothHciH4 = 187,

        /// <summary>
        /// ???
        /// </summary>
        Ieee80216MacCps = 188,

        /// <summary>
        /// USB packets, beginning with a Linux USB header, as specified by the struct usbmon_packet
        /// </summary>
        UsbLinux = 189,

        /// <summary>
        /// ???
        /// </summary>
        Can20B = 190,

        /// <summary>
        /// ???
        /// </summary>
        Ieee802154Linux = 191,

        /// <summary>
        /// Per-Packet Information information, as specified by the Per-Packet Information Header Specification, followed by a packet with the LINKTYPE_ value specified by the pph_dlt field of that header.
        /// </summary>
        Ppi = 192,

        /// <summary>
        /// ???
        /// </summary>
        Ieee80216MacCpsRadio = 193,

        /// <summary>
        /// ???
        /// </summary>
        JuniperIsm = 194,

        /// <summary>
        /// IEEE 802.15.4 Low-Rate Wireless Networks, with each packet having the FCS at the end of the frame.
        /// </summary>
        Ieee802154 = 195,

        /// <summary>
        /// Various link-layer types, with a pseudo-header, for SITA.
        /// </summary>
        Sita = 196,

        /// <summary>
        /// Various link-layer types, with a pseudo-header, for Endace DAG cards; encapsulates Endace ERF records.
        /// </summary>
        Erf = 197,

        /// <summary>
        /// ???
        /// </summary>
        Raif1 = 198,

        /// <summary>
        /// ???
        /// </summary>
        Ipmb = 199,

        /// <summary>
        /// ???
        /// </summary>
        JuniperSt = 200,

        /// <summary>
        /// Bluetooth HCI UART transport layer
        /// </summary>
        BluetoothHciH4WithPhdr = 201,

        /// <summary>
        /// AX.25 packet, with a 1-byte KISS header containing a type indicator.
        /// </summary>
        Ax25Kiss = 202,

        /// <summary>
        /// Link Access Procedures on the D Channel (LAPD) frames, as specified by ITU-T Recommendation Q.920 and ITU-T Recommendation Q.921, starting with the address field, with no pseudo-header.
        /// </summary>
        Lapd = 203,

        /// <summary>
        /// PPP, as per RFC 1661 and RFC 1662, preceded with a one-byte pseudo-header with a zero value meaning "received by this host" and a non-zero value meaning "sent by this host"
        /// </summary>
        PppWithDir = 204,

        /// <summary>
        /// Cisco PPP with HDLC framing, as per section 4.3.1 of RFC 1547
        /// </summary>
        CHdlcWithDir = 205,

        /// <summary>
        /// Frame Relay LAPF frames, beginning with a one-byte pseudo-header with a zero value meaning "received by this host" (DCE->DTE) and a non-zero value meaning "sent by this host" (DTE->DCE)
        /// </summary>
        FrelayWithDir = 206,

        /// <summary>
        /// Link Access Procedure, Balanced (LAPB), as specified by ITU-T Recommendation X.25
        /// </summary>
        LapbWithDir = 207,

        /// <summary>
        /// IPMB over an I2C circuit, with a Linux-specific pseudo-header
        /// </summary>
        IpmbLinux = 209,

        /// <summary>
        /// FlexRay automotive bus frames or symbols, preceded by a pseudo-header.
        /// </summary>
        Flexray = 210,

        /// <summary>
        /// ???
        /// </summary>
        Most = 211,

        /// <summary>
        /// ???
        /// </summary>
        Lin = 212,

        /// <summary>
        /// ???
        /// </summary>
        X2ESerial = 213,

        /// <summary>
        /// ???
        /// </summary>
        X2EXoraya = 214,

        /// <summary>
        /// IEEE 802.15.4 Low-Rate Wireless Networks, with each packet having the FCS at the end of the frame, and with the PHY-level data
        /// </summary>
        Ieee802154NonaskPhy = 215,

        /// <summary>
        /// ???
        /// </summary>
        LinuxEvdev = 216,

        /// <summary>
        /// ???
        /// </summary>
        GsmtapUm = 217,

        /// <summary>
        /// ???
        /// </summary>
        GsmtapAbis = 218,

        /// <summary>
        /// ???
        /// </summary>
        Mpls = 219,

        /// <summary>
        /// USB packets, beginning with a Linux USB header, as specified by the struct usbmon_packet
        /// </summary>
        UsbLinuxMmapped = 220,

        /// <summary>
        /// ???
        /// </summary>
        Dect = 221,

        /// <summary>
        /// ???
        /// </summary>
        Aos = 222,

        /// <summary>
        /// ???
        /// </summary>
        Wihart = 223,

        /// <summary>
        /// Fibre Channel FC-2 frames, beginning with a Frame_Header
        /// </summary>
        Fc2 = 224,

        /// <summary>
        /// Fibre Channel FC-2 frames, beginning an encoding of the SOF, followed by a Frame_Header, and ending with an encoding of the SOF
        /// </summary>
        Fc2WithFrameDelims = 225,

        /// <summary>
        /// Solaris ipnet pseudo-header, followed by an IPv4 or IPv6 datagram
        /// </summary>
        Ipnet = 226,

        /// <summary>
        /// CAN (Controller Area Network) frames, with a pseudo-header followed by the frame payload
        /// </summary>
        CanSocketcan = 227,

        /// <summary>
        /// Raw IPv4; the packet begins with an IPv4 header
        /// </summary>
        Ipv4 = 228,

        /// <summary>
        /// Raw IPv6; the packet begins with an IPv6 header
        /// </summary>
        Ipv6 = 229,

        /// <summary>
        /// IEEE 802.15.4 Low-Rate Wireless Network, without the FCS at the end of the frame
        /// </summary>
        Ieee802154Nofcs = 230,

        /// <summary>
        /// Raw D-Bus messages
        /// </summary>
        Dbus = 231,

        /// <summary>
        /// ???
        /// </summary>
        JuniperVs = 232,

        /// <summary>
        /// ???
        /// </summary>
        JuniperSrxE2E = 233,

        /// <summary>
        /// ???
        /// </summary>
        JuniperFibrechannel = 234,

        /// <summary>
        /// DVB-CI (DVB Common Interface for communication between a PC Card module and a DVB receiver)
        /// </summary>
        DvbCi = 235,

        /// <summary>
        /// Variant of 3GPP TS 27.010 multiplexing protocol 
        /// </summary>
        Mux27010 = 236,

        /// <summary>
        /// D_PDUs as described by NATO standard STANAG 5066
        /// </summary>
        Stanag5066DPdu = 237,

        /// <summary>
        /// ???
        /// </summary>
        JuniperAtmCemic = 238,

        /// <summary>
        /// Linux netlink NETLINK NFLOG socket log messages
        /// </summary>
        Nflog = 239,

        /// <summary>
        /// Pseudo-header for Hilscher Gesellschaft für Systemautomation mbH netANALYZER devices
        /// </summary>
        Netanalyzer = 240,

        /// <summary>
        /// Pseudo-header for Hilscher Gesellschaft für Systemautomation mbH netANALYZER devices, beginning with the preamble
        /// </summary>
        NetanalyzerTransparent = 241,

        /// <summary>
        /// IP-over-InfiniBand, as specified by RFC 4391 section 6
        /// </summary>
        Ipoib = 242,

        /// <summary>
        /// MPEG-2 Transport Stream transport packets, as specified by ISO 13818-1/ITU-T Recommendation H.222.0 
        /// </summary>
        Mpeg2Ts = 243,

        /// <summary>
        /// Pseudo-header for ng4T GmbH's UMTS Iub/Iur-over-ATM and Iub/Iur-over-IP format as used by their ng40 protocol tester
        /// </summary>
        Ng40 = 244,

        /// <summary>
        /// Pseudo-header for NFC LLCP packet captures
        /// </summary>
        NfcLlcp = 245,

        /// <summary>
        /// ???
        /// </summary>
        Pfsync = 246,

        /// <summary>
        /// Raw InfiniBand frames
        /// </summary>
        Infiniband = 247,

        /// <summary>
        /// SCTP packets, as defined by RFC 4960
        /// </summary>
        Sctp = 248,

        /// <summary>
        /// USB packets, beginning with a USBPcap header
        /// </summary>
        USBPcap = 249,

        /// <summary>
        /// Serial-line packet header for the Schweitzer Engineering Laboratories "RTAC" product
        /// </summary>
        RtacSerial = 250,

        /// <summary>
        /// Bluetooth Low Energy air interface Link Layer packets
        /// </summary>
        BluetoothLeLl = 251,

        /// <summary>
        /// ???
        /// </summary>
        WiresharkUpperPdu = 252,

        /// <summary>
        /// Linux Netlink capture encapsulation
        /// </summary>
        Netlink = 253,

        /// <summary>
        /// Bluetooth Linux Monitor encapsulation of traffic for the BlueZ stack
        /// </summary>
        BluetoothLinuxMonitor = 254,

        /// <summary>
        /// Bluetooth Basic Rate and Enhanced Data Rate baseband packets
        /// </summary>
        BluetoothBredrBb = 255,

        /// <summary>
        /// Bluetooth Low Energy link-layer packets
        /// </summary>
        BluetoothLeLlWithPhdr = 256,

        /// <summary>
        /// PROFIBUS data link layer packets
        /// </summary>
        ProfibusDl = 257,

        /// <summary>
        /// Apple PKTAP capture encapsulation
        /// </summary>
        Pktap = 258,

        /// <summary>
        /// Ethernet-over-passive-optical-network packets
        /// </summary>
        Epon = 259,

        /// <summary>
        /// IPMI trace packets
        /// </summary>
        IpmiHpm2 = 260,

        /// <summary>
        /// Z-Wave RF profile R1 and R2 packets
        /// </summary>
        ZwaveR1R2 = 261,

        /// <summary>
        /// Z-Wave RF profile R3 packets
        /// </summary>
        ZwaveR3 = 262,

        /// <summary>
        /// Formats for WattStopper Digital Lighting Management (DLM) and Legrand Nitoo Open protocol common packet structure captures
        /// </summary>
        WattstopperDlm = 263,

        /// <summary>
        /// Messages between ISO 14443 contactless smartcards
        /// </summary>
        Iso14443 = 264,

        /// <summary>
        /// Radio data system (RDS) groups
        /// </summary>
        Rds = 265,

        /// <summary>
        /// USB packets, beginning with a Darwin (macOS, etc.) USB header
        /// </summary>
        UsbDarwin = 266,

        /// <summary>
        /// SDLC packets
        /// </summary>
        Sdlc = 268,

        /// <summary>
        /// LoRaTap pseudo-header
        /// </summary>
        Loratap = 270,

        /// <summary>
        /// Protocol for communication between host and guest machines in VMware and KVM hypervisors
        /// </summary>
        Vsock = 271,

        /// <summary>
        /// Messages to and from a Nordic Semiconductor nRF Sniffer for Bluetooth LE packets
        /// </summary>
        NordicBle = 272,

        /// <summary>
        /// DOCSIS packets and bursts
        /// </summary>
        Docsis31Xra31 = 273,

        /// <summary>
        /// mPackets
        /// </summary>
        EthernetMpacket = 274,

        /// <summary>
        /// DisplayPort AUX channel monitoring data as specified by VESA DisplayPort(DP) Standard
        /// </summary>
        DisplayportAux = 275,

        /// <summary>
        /// Linux "cooked" capture encapsulation v2
        /// </summary>
        LinuxSll2 = 276,

        /// <summary>
        /// Openvizsla FPGA-based USB sniffer
        /// </summary>
        Openvizsla = 278,

        /// <summary>
        /// Elektrobit High Speed Capture and Replay (EBHSCR) format
        /// </summary>
        Ebhscr = 279,

        /// <summary>
        /// Records in traces from the http://fd.io VPP graph dispatch tracer
        /// </summary>
        VppDispatch = 280,

        /// <summary>
        /// Ethernet frames, with a switch tag inserted between the source address field and the type/length field in the Ethernet header
        /// </summary>
        DsaTagBrcm = 281,

        /// <summary>
        /// Ethernet frames, with a switch tag inserted before the destination address in the Ethernet header
        /// </summary>
        DsaTagBrcmPrepend = 282,

        /// <summary>
        /// IEEE 802.15.4 Low-Rate Wireless Networks, with a pseudo-header containing TLVs with metadata preceding the 802.15.4 header
        /// </summary>
        Ieee802154Tap = 283,

        /// <summary>
        /// Ethernet frames, with a switch tag inserted between the source address field and the type/length field in the Ethernet header
        /// </summary>
        DsaTagDsa = 284,

        /// <summary>
        /// Ethernet frames, with a programmable Ethernet type switch tag inserted between the source address field and the type/length field in the Ethernet header
        /// </summary>
        DsaTagEdsa = 285,

        /// <summary>
        /// Payload of lawful intercept packets using the ELEE protocol
        /// </summary>
        Elee = 286,

        /// <summary>
        /// Serial frames transmitted between a host and a Z-Wave chip over an RS-232 or USB serial connection
        /// </summary>
        ZWaveSerial = 287,

        /// <summary>
        /// USB 2.0, 1.1, or 1.0 packet
        /// </summary>
        Usb20 = 288,

        /// <summary>
        /// ATSC Link-Layer Protocol frames
        /// </summary>
        AtscAlp = 289,

        /// <summary>
        /// Event Tracing for Windows messages, beginning with a pseudo-header.
        /// </summary>
        Etw = 290,
    }
}
