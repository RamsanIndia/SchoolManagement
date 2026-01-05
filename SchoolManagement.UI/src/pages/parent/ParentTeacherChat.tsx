import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  ArrowLeft,
  Send,
  Paperclip,
  MoreVertical,
  Phone,
  Video,
  Search,
  Check,
  CheckCheck,
} from "lucide-react";

interface Message {
  id: string;
  sender: "parent" | "teacher";
  content: string;
  timestamp: string;
  read: boolean;
  attachment?: {
    name: string;
    type: string;
  };
}

interface Conversation {
  id: string;
  teacherName: string;
  subject: string;
  lastMessage: string;
  lastMessageTime: string;
  unread: number;
  avatar: string;
}

const conversations: Conversation[] = [
  {
    id: "1",
    teacherName: "Mrs. Priya Sharma",
    subject: "Class Teacher",
    lastMessage: "Thank you for the update. Rohan is doing well.",
    lastMessageTime: "10:30 AM",
    unread: 2,
    avatar: "",
  },
  {
    id: "2",
    teacherName: "Mr. Rajesh Kumar",
    subject: "Mathematics",
    lastMessage: "Please help Rohan practice more word problems.",
    lastMessageTime: "Yesterday",
    unread: 0,
    avatar: "",
  },
  {
    id: "3",
    teacherName: "Mrs. Anjali Gupta",
    subject: "English",
    lastMessage: "The essay submission was excellent!",
    lastMessageTime: "Dec 5",
    unread: 0,
    avatar: "",
  },
];

const mockMessages: Message[] = [
  {
    id: "1",
    sender: "teacher",
    content: "Good morning! I wanted to discuss Rohan's progress in class.",
    timestamp: "9:00 AM",
    read: true,
  },
  {
    id: "2",
    sender: "parent",
    content: "Good morning Mrs. Sharma. Yes, please go ahead.",
    timestamp: "9:05 AM",
    read: true,
  },
  {
    id: "3",
    sender: "teacher",
    content: "Rohan has been performing very well in his studies. His recent test scores have improved significantly.",
    timestamp: "9:10 AM",
    read: true,
  },
  {
    id: "4",
    sender: "teacher",
    content: "However, I noticed he's been a bit distracted during class lately. Is everything okay at home?",
    timestamp: "9:12 AM",
    read: true,
  },
  {
    id: "5",
    sender: "parent",
    content: "Thank you for letting me know. We'll talk to him about staying focused in class.",
    timestamp: "9:20 AM",
    read: true,
  },
  {
    id: "6",
    sender: "teacher",
    content: "That would be helpful. Also, please find attached his progress report for this month.",
    timestamp: "9:25 AM",
    read: true,
    attachment: {
      name: "Progress_Report_Nov.pdf",
      type: "pdf",
    },
  },
  {
    id: "7",
    sender: "parent",
    content: "Thank you for the update. We'll review the report and work on the areas that need improvement.",
    timestamp: "10:30 AM",
    read: false,
  },
];

export default function ParentTeacherChat() {
  const navigate = useNavigate();
  const [selectedConversation, setSelectedConversation] = useState<Conversation | null>(conversations[0]);
  const [messages, setMessages] = useState<Message[]>(mockMessages);
  const [newMessage, setNewMessage] = useState("");
  const [searchTerm, setSearchTerm] = useState("");

  const filteredConversations = conversations.filter((c) =>
    c.teacherName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    c.subject.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleSendMessage = () => {
    if (!newMessage.trim()) return;

    const message: Message = {
      id: (messages.length + 1).toString(),
      sender: "parent",
      content: newMessage,
      timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }),
      read: false,
    };

    setMessages([...messages, message]);
    setNewMessage("");
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Messages</h1>
          <p className="text-muted-foreground">Communicate with your child's teachers</p>
        </div>
      </div>

      {/* Chat Interface */}
      <div className="grid md:grid-cols-3 gap-4 h-[calc(100vh-220px)]">
        {/* Conversations List */}
        <Card className="md:col-span-1">
          <CardHeader className="pb-3">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search conversations..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
          </CardHeader>
          <CardContent className="p-0">
            <ScrollArea className="h-[calc(100vh-340px)]">
              {filteredConversations.map((conversation) => (
                <div
                  key={conversation.id}
                  className={`flex items-center gap-3 p-4 cursor-pointer hover:bg-muted/50 border-b ${
                    selectedConversation?.id === conversation.id ? "bg-muted" : ""
                  }`}
                  onClick={() => setSelectedConversation(conversation)}
                >
                  <Avatar>
                    <AvatarImage src={conversation.avatar} />
                    <AvatarFallback>
                      {conversation.teacherName.split(" ").map((n) => n[0]).join("")}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex-1 min-w-0">
                    <div className="flex justify-between items-center">
                      <p className="font-medium truncate">{conversation.teacherName}</p>
                      <span className="text-xs text-muted-foreground">{conversation.lastMessageTime}</span>
                    </div>
                    <p className="text-sm text-muted-foreground truncate">{conversation.subject}</p>
                    <p className="text-sm text-muted-foreground truncate">{conversation.lastMessage}</p>
                  </div>
                  {conversation.unread > 0 && (
                    <Badge className="bg-primary">{conversation.unread}</Badge>
                  )}
                </div>
              ))}
            </ScrollArea>
          </CardContent>
        </Card>

        {/* Chat Area */}
        <Card className="md:col-span-2 flex flex-col">
          {selectedConversation ? (
            <>
              {/* Chat Header */}
              <CardHeader className="border-b">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <Avatar>
                      <AvatarFallback>
                        {selectedConversation.teacherName.split(" ").map((n) => n[0]).join("")}
                      </AvatarFallback>
                    </Avatar>
                    <div>
                      <CardTitle className="text-lg">{selectedConversation.teacherName}</CardTitle>
                      <p className="text-sm text-muted-foreground">{selectedConversation.subject}</p>
                    </div>
                  </div>
                  <div className="flex gap-2">
                    <Button variant="ghost" size="icon">
                      <Phone className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon">
                      <Video className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon">
                      <MoreVertical className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </CardHeader>

              {/* Messages */}
              <ScrollArea className="flex-1 p-4">
                <div className="space-y-4">
                  {messages.map((message) => (
                    <div
                      key={message.id}
                      className={`flex ${message.sender === "parent" ? "justify-end" : "justify-start"}`}
                    >
                      <div
                        className={`max-w-[70%] rounded-lg p-3 ${
                          message.sender === "parent"
                            ? "bg-primary text-primary-foreground"
                            : "bg-muted"
                        }`}
                      >
                        <p>{message.content}</p>
                        {message.attachment && (
                          <div className="mt-2 p-2 rounded bg-background/10 flex items-center gap-2">
                            <Paperclip className="h-4 w-4" />
                            <span className="text-sm">{message.attachment.name}</span>
                          </div>
                        )}
                        <div className={`flex items-center justify-end gap-1 mt-1 ${
                          message.sender === "parent" ? "text-primary-foreground/70" : "text-muted-foreground"
                        }`}>
                          <span className="text-xs">{message.timestamp}</span>
                          {message.sender === "parent" && (
                            message.read ? (
                              <CheckCheck className="h-3 w-3" />
                            ) : (
                              <Check className="h-3 w-3" />
                            )
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </ScrollArea>

              {/* Message Input */}
              <div className="p-4 border-t">
                <div className="flex gap-2">
                  <Button variant="ghost" size="icon">
                    <Paperclip className="h-4 w-4" />
                  </Button>
                  <Input
                    placeholder="Type a message..."
                    value={newMessage}
                    onChange={(e) => setNewMessage(e.target.value)}
                    onKeyPress={(e) => e.key === "Enter" && handleSendMessage()}
                    className="flex-1"
                  />
                  <Button onClick={handleSendMessage}>
                    <Send className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </>
          ) : (
            <div className="flex-1 flex items-center justify-center text-muted-foreground">
              Select a conversation to start messaging
            </div>
          )}
        </Card>
      </div>
    </div>
  );
}
