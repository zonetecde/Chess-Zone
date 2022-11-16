﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessZone
{
    public class ChessMessage
    {
        public ChessMessage(CHESS_MESSAGE_TYPE messageType, string senderId, string message)
        {
            MessageType = messageType;
            SenderId = senderId;
            Message = message;
        }

        public CHESS_MESSAGE_TYPE MessageType { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
    }

    public enum CHESS_MESSAGE_TYPE
    {
    }
}
