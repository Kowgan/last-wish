using System;
using Server.Network;
using Server.Commands;
using System.Collections.Generic;

namespace Server.Gumps
{
    public class SIMGumpList : Gump
    {
        public static void Initialize()
        {
            CommandSystem.Register("PM", AccessLevel.Player, new CommandEventHandler(SIM_OnCommand));
        }

        [Usage("PM [filter]")]
        [Description("Send a private message to any online player.")]
        private static void SIM_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new SIMGumpList(e.Mobile, e.ArgString));
        }

        public static bool OldStyle = PropsConfig.OldStyle;

        public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
        public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

        public static readonly int TextHue = PropsConfig.TextHue;
        public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

        public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
        public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
        public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
        public static readonly int BackGumpID = PropsConfig.BackGumpID;
        public static readonly int SetGumpID = PropsConfig.SetGumpID;

        public static readonly int SetWidth = PropsConfig.SetWidth;
        public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
        public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
        public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

        public static readonly int PrevWidth = PropsConfig.PrevWidth;
        public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
        public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public static readonly int NextWidth = PropsConfig.NextWidth;
        public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
        public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
        public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

        public static readonly int OffsetSize = PropsConfig.OffsetSize;

        public static readonly int EntryHeight = PropsConfig.EntryHeight;
        public static readonly int BorderSize = PropsConfig.BorderSize;

        private static bool PrevLabel = false, NextLabel = false;

        private static readonly int PrevLabelOffsetX = PrevWidth + 1;
        private static readonly int PrevLabelOffsetY = 0;

        private static readonly int NextLabelOffsetX = -29;
        private static readonly int NextLabelOffsetY = 0;

        private static readonly int EntryWidth = 180;
        private static readonly int EntryCount = 15;

        private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
        private static readonly int TotalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (EntryCount + 1));

        private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
        private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

        private Mobile m_Owner;
        private List<Mobile> m_Mobiles;
        private int m_Page;

        private class InternalComparer : IComparer<Mobile>
        {
            public static readonly IComparer<Mobile> Instance = new InternalComparer();

            public InternalComparer()
            {
            }

            public int Compare(Mobile x, Mobile y)
            {
                if (x == null || y == null)
                    throw new ArgumentException();

                if (x.AccessLevel > y.AccessLevel)
                    return -1;
                else if (x.AccessLevel < y.AccessLevel)
                    return 1;
                else
                    return Insensitive.Compare(x.Name, y.Name);
            }
        }

        public SIMGumpList(Mobile owner, string filter)
            : this(owner, BuildList(owner, filter), 0)
        {
        }

        public SIMGumpList(Mobile owner, List<Mobile> list, int page)
            : base(GumpOffsetX, GumpOffsetY)
        {
            owner.CloseGump(typeof(SIMGumpList));

            m_Owner = owner;
            m_Mobiles = list;

            if (m_Mobiles.Count == 1)
            {
                owner.SendGump(new SIMGumpChatPlayer(owner, m_Mobiles[0].NetState));
            }
            else
            {
                Initialize(page);
            }
        }

        public static List<Mobile> BuildList(Mobile owner, string filter)
        {
            if (filter != null && (filter = filter.Trim()).Length == 0)
                filter = null;
            else
                filter = filter.ToLower();

            List<Mobile> list = new List<Mobile>();
            List<NetState> states = NetState.Instances;

            for (int i = 0; i < states.Count; ++i)
            {
                Mobile m = states[i].Mobile;

                if (m != null)
                {
                    if (filter != null && (m.Name == null || m.Name.ToLower().IndexOf(filter) < 0))
                        continue;

                    list.Add(m);
                }
            }

            list.Sort(InternalComparer.Instance);

            return list;
        }

        public void Initialize(int page)
        {
            m_Page = page;

            int count = m_Mobiles.Count - (page * EntryCount);

            if (count < 0)
                count = 0;
            else if (count > EntryCount)
                count = EntryCount;

            int totalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (count + 1));

            AddPage(0);

            AddBackground(0, 0, BackWidth, BorderSize + totalHeight + BorderSize, BackGumpID);
            AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), totalHeight, OffsetGumpID);

            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize;

            int emptyWidth = TotalWidth - PrevWidth - NextWidth - (OffsetSize * 4) - (OldStyle ? SetWidth + OffsetSize : 0);

            if (!OldStyle)
                AddImageTiled(x - (OldStyle ? OffsetSize : 0), y, emptyWidth + (OldStyle ? OffsetSize * 2 : 0), EntryHeight, EntryGumpID);

            AddLabel(x + TextOffsetX, y, TextHue, String.Format("Page {0} of {1} ({2})", page + 1, (m_Mobiles.Count + EntryCount - 1) / EntryCount, m_Mobiles.Count));

            x += emptyWidth + OffsetSize;

            if (OldStyle)
                AddImageTiled(x, y, TotalWidth - (OffsetSize * 3) - SetWidth, EntryHeight, HeaderGumpID);
            else
                AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);

            if (page > 0)
            {
                AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 1, GumpButtonType.Reply, 0);

                if (PrevLabel)
                    AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
            }

            x += PrevWidth + OffsetSize;

            if (!OldStyle)
                AddImageTiled(x, y, NextWidth, EntryHeight, HeaderGumpID);

            if ((page + 1) * EntryCount < m_Mobiles.Count)
            {
                AddButton(x + NextOffsetX, y + NextOffsetY, NextButtonID1, NextButtonID2, 2, GumpButtonType.Reply, 1);

                if (NextLabel)
                    AddLabel(x + NextLabelOffsetX, y + NextLabelOffsetY, TextHue, "Next");
            }

            for (int i = 0, index = page * EntryCount; i < EntryCount && index < m_Mobiles.Count; ++i, ++index)
            {
                x = BorderSize + OffsetSize;
                y += EntryHeight + OffsetSize;

                Mobile m = m_Mobiles[index];

                AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
                AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, GetHueFor(m), m.Deleted ? "(deleted)" : m.Name);

                x += EntryWidth + OffsetSize;

                if (SetGumpID != 0)
                    AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

                if (m.NetState != null && !m.Deleted)
                    AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, i + 3, GumpButtonType.Reply, 0);
            }
        }

        private static int GetHueFor(Mobile m)
        {
            switch (m.AccessLevel)
            {
                case AccessLevel.Owner:
                case AccessLevel.Developer:
                case AccessLevel.Administrator: return 0x516;
                case AccessLevel.Seer: return 0x144;
                case AccessLevel.GameMaster: return 0x21;
                case AccessLevel.Counselor: return 0x2;
                case AccessLevel.Player:
                default:
                    {
                        if (m.Kills >= 5)
                            return 0x21;
                        else if (m.Criminal)
                            return 0x3B1;

                        return 0x58;
                    }
            }
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            switch (info.ButtonID)
            {
                case 0: // Closed
                    {
                        return;
                    }
                case 1: // Previous
                    {
                        if (m_Page > 0)
                            from.SendGump(new SIMGumpList(from, m_Mobiles, m_Page - 1));

                        break;
                    }
                case 2: // Next
                    {
                        if ((m_Page + 1) * EntryCount < m_Mobiles.Count)
                            from.SendGump(new SIMGumpList(from, m_Mobiles, m_Page + 1));

                        break;
                    }
                default:
                    {
                        int index = (m_Page * EntryCount) + (info.ButtonID - 3);

                        if (index >= 0 && index < m_Mobiles.Count)
                        {
                            Mobile m = m_Mobiles[index];

                            if (m.Deleted)
                            {
                                from.SendMessage("That player has deleted their character.");
                                from.SendGump(new SIMGumpList(from, m_Mobiles, m_Page));
                            }
                            else if (m.NetState == null)
                            {
                                from.SendMessage("That player is no longer online.");
                                from.SendGump(new SIMGumpList(from, m_Mobiles, m_Page));
                            }
                            else
                            {
                                from.SendGump(new SIMGumpChatPlayer(from, m.NetState));
                            }
                        }

                        break;
                    }
            }
        }
    }

    public class SIMGumpChatPlayer : Gump
    {
        private NetState m_State;
        
        public SIMGumpChatPlayer(Mobile from, NetState state)
            : base(0, 0)
        {
            Mobile m = state.Mobile;

            m_State = state;
            
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            this.AddPage(0);
            this.AddBackground(79, 44, 290, 297, 5170);
            this.AddLabel(119, 73, 57, @"To: " + m.Name);
            this.AddButton(269, 73, 247, 248, 1, GumpButtonType.Reply, 0);
            this.AddImageTiled(116, 99, 220, 169, 0xA40);
            this.AddImageTiled(117, 100, 218, 167, 0xBBC);
            this.AddTextEntry(119, 100, 216, 167, 0x480, 0, "");
            this.AddImage(123, 280, Utility.RandomMinMax(30010, 30057));
            this.AddLabel(155, 285, 1117, @"Click <OKAY> to send!");
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (m_State == null || info.GetTextEntry(0).Text == "")
                return;

            Mobile focus = m_State.Mobile;
            Mobile from = state.Mobile;

            if (focus == null)
            {
                from.SendMessage("That character is no longer online.");
                return;
            }
            else if (focus.Deleted)
            {
                from.SendMessage("That character no longer exists.");
                return;
            }
            /*
            else if (from != focus && focus.Hidden && from.AccessLevel < focus.AccessLevel)
            {
                from.SendMessage("That character is no longer visible.");
                return;
            }
            */

            if (info.ButtonID == 1)
            {
                TextRelay text = info.GetTextEntry(0);

                if (text != null)
                {
                    focus.SendMessage(0x482, "{0} tells you:", from.Name);
                    focus.SendMessage(0x482, text.Text);

                    string Author = from.Name;
                    string Message = text.Text;

                    focus.SendGump(new SIMGumpSend(Author, Message));

                    // CommandLogging.WriteLine(from, "{0} {1} telling {2} \"{3}\" ", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(focus), text.Text);
                }

                from.SendGump(new SIMGumpChatPlayer(from, m_State));
            }
        }
    }

    public class SIMGumpSend : Gump
    {
        public SIMGumpSend(string Author, string Message)
            : base(0, 0)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            this.AddPage(0);
            this.AddBackground(79, 44, 290, 297, 5170);
            this.AddLabel(119, 73, 57, @"From: " + Author);
            this.AddButton(269, 73, 247, 248, (int)Buttons.Button1, GumpButtonType.Reply, 0);
            this.AddHtml(119, 100, 216, 167, @"" + Message, (bool)true, (bool)true);
            this.AddImage(123, 280, Utility.RandomMinMax(30010, 30057));
            this.AddLabel(155, 285, 1117, @"[PM " + Author + " to reply!");
        }

        public enum Buttons
        {
            Button1,
        }
    }
}