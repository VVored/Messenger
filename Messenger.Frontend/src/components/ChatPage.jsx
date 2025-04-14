import React, { useEffect, useState } from 'react'
import axios from 'axios';
import ChatList from './ChatList';
import ChatMessages from './ChatMessages';
import CreateChat from './CreateChat';
import { HubConnectionBuilder } from "@microsoft/signalr";
import Profile from './Profile';
import notificationSound from '../notification_sound.mp3';


const ChatPage = () => {
    const [chats, setChats] = useState([]);
    const [createChatIsOpen, setCreateChatIsOpen] = useState(false);
    const [selectedChat, setSelectedChat] = useState(null);
    const [connection, setConnection] = useState(null);
    const [selectedUser, setSelectedUser] = useState(null);
    const audio = new Audio(notificationSound);

    const StartConnection = async () => {
        const connection = new HubConnectionBuilder()
            .withUrl(`https://localhost:7192/chat?access_token=${localStorage.getItem('token')}`)
            .withAutomaticReconnect()
            .build();
        await connection.start();
        connection.invoke('JoinBasicGroups');
        connection.off('createAndSetPrivateChat');
        connection.on('createAndSetPrivateChat', async (response) => {
            setChats(prev => [...prev, response]);
            openChatMessages(response);
        });
        connection.off('createPrivateChat');
        connection.on('createPrivateChat', async (response) => {
            setChats(prev => [...prev, response]);
        });
        connection.off('notification');
        connection.on('notification', async (response) => {
            setChats(prev => prev.map(chat => chat.chatId === response.chatId ? { ...chat, lastMessage: response } : chat));
        })
        connection.off('notificationSound');
        connection.on('notificationSound', () => {
            audio.play();
        })

        setConnection(connection);
    }

    const JoinGroup = async (chatId) => {
        connection.invoke('JoinGroup', chatId + '');
    }

    const LeaveGroup = async (chatId) => {
        connection.invoke('LeaveGroup', chatId + '');
    }

    const getUserChats = async () => {
        const token = localStorage.getItem('token');
        const response = await axios.get('https://localhost:7192/api/chats/my', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        return response;
    }

    const openCreateChat = () => {
        if (selectedChat) {
            LeaveGroup(selectedChat.chatId);
        }
        setSelectedChat(null);
        setCreateChatIsOpen(true);
    }

    const openChatMessages = (chat) => {
        if (selectedChat !== null) {
            LeaveGroup(selectedChat.chatId);
        }
        JoinGroup(chat.chatId);
        setSelectedChat(chat);
        setCreateChatIsOpen(false);
    }

    useEffect(() => {
        StartConnection();
        getUserChats().then((result) => {
            setChats(result.data);
        });
        audio.volume = 0.2;
    }, []);

    return (
        <div style={{ display: "flex", height: '100vh', overflowY: 'hidden' }}>
            <ChatList chats={chats} openCreateChat={openCreateChat} openChatMessages={openChatMessages} setSelectedUser={setSelectedUser} />
            <div style={{ width: '100%', backgroundColor: "rgba(178, 178, 178, 0.5)" }}>
                {
                    createChatIsOpen ? <CreateChat setChats={setChats} setSelectedChat={setSelectedChat} setCreateChatIsOpen={setCreateChatIsOpen} JoinGroup={JoinGroup} /> : selectedChat ? <ChatMessages connection={connection} setSelectedUser={setSelectedUser} setChats={setChats} chat={selectedChat}></ChatMessages> : <h1 style={{ textAlign: "center" }}>Выберите чат</h1>
                }
            </div>
            {
                selectedUser
                    ? <Profile selectedUser={selectedUser} setSelectedUser={setSelectedUser} connection={connection} chats={chats} openChatMessages={openChatMessages}></Profile>
                    : <div></div>
            }
        </div>
    )
}

export default ChatPage;