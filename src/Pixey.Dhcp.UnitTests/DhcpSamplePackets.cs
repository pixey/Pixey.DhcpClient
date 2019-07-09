﻿using Pixey.Dhcp.UnitTests.Extensions;

namespace Pixey.Dhcp.UnitTests
{
    internal static class DhcpSamplePackets
    {
        private static readonly string OfferHexString = "02010600001b0c610000800000000000c0a80165c0a8010d00000000deadc0decafe0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000697078652e656669000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000638253633501023604c0a801023304000002580104ffffff000304c0a80101060408080808ff0000000000000000000000000000000000000000000000000000";
        private static readonly string DiscoverHexString = "01010600001b0c610000800000000000000000000000000000000000deadc0decafe0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000063825363350101ff";
        private static readonly string RequestHexString = "010106005e005030000480000000000000000000000000000000000000155d00503000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000638253633501033204c0a80164371801020305060b0c0d0f1011122b363c438081828384858687390204ec3604c0a8010261110086257cde4edc8a4e8940a4847f362d095d0200005e030102013c20505845436c69656e743a417263683a30303030303a554e44493a303032303031ff0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
        private static readonly string AckHexString = "020106005e0050300004800000000000c0a80164c0a8010d0000000000155d0050300000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000756e64696f6e6c792e6b70786500000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000638253633501053604c0a801023304000002580104ffffff000304c0a80101060408080808ff0000000000000000000000000000000000000000000000000000";

        private static readonly string DiscoverLegacyBiosX86String = "010106005e005030000480000000000000000000000000000000000000155d0050300000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000063825363350101371801020305060b0c0d0f1011122b363c438081828384858687390204ec61110086257cde4edc8a4e8940a4847f362d095d0200005e030102013c20505845436c69656e743a417263683a30303030303a554e44493a303032303031ff0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
        private static readonly string DiscoverEfiX64String = "0101060084a0d1b2000080000000000000000000000000000000000000155d00502f0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000063825363350101390205c037230102030405060c0d0f111216171c28292a2b3233363a3b3c42436180818283848586876111000041974ee9607f46b5bd093282c87e4f5e030103005d0200073c20505845436c69656e743a417263683a30303030373a554e44493a303033303030ff";

        public static readonly byte[] Offer = OfferHexString.AsHexBytes();
        public static readonly byte[] Discover = DiscoverHexString.AsHexBytes();
        public static readonly byte[] Request = RequestHexString.AsHexBytes();
        public static readonly byte[] Ack = AckHexString.AsHexBytes();

        public static readonly byte[] DiscoverLegacyBiosX86 = DiscoverLegacyBiosX86String.AsHexBytes();
        public static readonly byte[] DiscoverEfiX64 = DiscoverEfiX64String.AsHexBytes();
    }
}