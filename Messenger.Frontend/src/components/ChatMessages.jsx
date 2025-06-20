import React, { useEffect, useRef, useState } from 'react'
import axios from 'axios';
import Message from './Message';
import styles from './ChatMessages.module.css';
import styleChat from './Chat.module.css';
import leaveChatImg from '../imgs/leave_chat.png';
import blockUserImg from '../imgs/ban.png';
import unblockUserImg from '../imgs/unban.png';
import FileAttachment from './FileAttachment';
import { jwtDecode } from 'jwt-decode';
import EmojiList from './EmojiList';

const ChatMessages = ({ connection, setSelectedUser, chat, setChats }) => {

    const [messages, setMessages] = useState([]);
    const messagesEndRef = useRef(null);
    const currentMessageInput = useRef(null);
    const [currentMessage, setCurrentMessage] = useState('');
    const [isJoined, setIsJoined] = useState(false);
    const [isUserBlocked, setIsUserBlocked] = useState(false);
    const [amIBlocked, setAmIBlocked] = useState(false);
    const [amountOfVh, setAmountOfVh] = useState(85);
    const [files, setFiles] = useState([]);
    const [privateChatAvatarUrl, setPrivateChatAvatarUrl] = useState('');
    const [privateChatName, setPrivateChatName] = useState('');
    const [privateChatUserInfo, setPrivateChatUserInfo] = useState(null);
    const [isEmojiListOpen, setIsEmojiListOpen] = useState(false);
    const [repliableMessage, setRepliableMessage] = useState(null);

    const scrollToBottom = () => {
        messagesEndRef.current.scrollIntoView({ behaivor: 'smooth' });
    }

    const IsUserJoined = async () => {
        const token = localStorage.getItem('token');
        const response = await axios.get(`http://45.144.222.67:5266/api/chats/${chat.chatId}/members/isjoined`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        })
        setIsJoined(response.data);
    }

    const IsUserBlocked = async (userId) => {
        const token = localStorage.getItem('token');
        const response = await axios.get(`http://45.144.222.67:5266/api/block/isblocked/${userId}/`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        })
        console.log(response.data);
        setIsUserBlocked(response.data);
    }

    const AmIBlocked = async (userId) => {
        const token = localStorage.getItem('token');
        const response = await axios.get(`http://45.144.222.67:5266/api/block/amiblocked/${userId}/`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        console.log(response.data);
        setAmIBlocked(response.data);
    }

    const getChatMessages = async (chatId) => {
        const token = localStorage.getItem('token');
        const response = await axios.get(`http://45.144.222.67:5266/api/messages/chats/${chatId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        })
        return response;
    }

    const ConfigureConnection = async () => {
        connection.off('leaveChatMember');
        connection.off('userJoinChat');
        connection.off('newMessage');
        connection.on('newMessage', message => {
            setMessages(prev => [...prev, message]);
        });
        connection.on('userJoinChat', response => {
            setChats(prev => [...prev, chat]);
            setIsJoined(true);
        });
        connection.on('leaveChatMember', response => {
            setChats(prev => prev.filter(item => item !== chat));
            setIsJoined(false);
        })
    }

    const sendMessage = async (e, chatId, content, repliableMessageId) => {
        if (e.key === 'Enter') {
            if (content !== '' || files.length > 0) {
                const token = localStorage.getItem('token');
                const attachments = await uploadFiles(files);
                const response = await fetch('http://45.144.222.67:5266/api/messages', {
                    method: 'POST',
                    body: JSON.stringify({ chatId, content, attachments, repliableMessageId }),
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                });
                setFiles([]);
                setCurrentMessage('');
                setRepliableMessage(null);
                currentMessageInput.current.value = '';
                await connection.invoke('SendNotificationSound', chatId + 'Notification');
            }
        }
    }

    const onPasteImage = (e) => {
        if (e.clipboardData) {
            var items = e.clipboardData.items;
            if (items) {
                for (let i = 0; i < items.length; i++) {
                    if (items[i].type.indexOf('image') !== -1) {
                        var file = items[i].getAsFile();
                        setFiles(prev => [...prev, file]);
                    }
                }
            }
        }
    }

    const uploadFiles = async (files) => {
        let attachments = [];
        for (let i = 0; i < files.length; i++) {
            const formData = new FormData();
            formData.append('file', files[i]);
            const response = await axios.post('http://45.144.222.67:5266/api/files', formData);
            const attachment = {
                fileUrl: response.data,
                fileType: files[i].type,
                fileSize: files[i].size
            }
            attachments = [...attachments, attachment];
        }
        return attachments;
    }

    const JoinChat = async () => {
        connection.invoke('JoinChat', chat.chatId + '');
    }

    const LeaveChat = async () => {
        connection.invoke('LeaveChat', chat.chatId + '');
    }

    const BlockUser = async () => {
        const token = localStorage.getItem('token');
        await fetch(`http://45.144.222.67:5266/api/block/${privateChatUserInfo.userId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
            }
        });
        setIsUserBlocked(true);
    }

    const UnblockUser = async () => {
        const token = localStorage.getItem('token');
        await fetch(`http://45.144.222.67:5266/api/block/${privateChatUserInfo.userId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`,
            }
        });
        setIsUserBlocked(false);
    }

    useEffect(() => {
        ConfigureConnection();
        setIsJoined(IsUserJoined());
        getChatMessages(chat.chatId).then(response => { setMessages(response.data) });
        if (chat.chatType === 'private') {
            setPrivateChatAvatarUrl('');
            setPrivateChatName('');
            setPrivateChatUserInfo(null);
            const token = localStorage.getItem('token');
            const decoded = jwtDecode(token);
            chat.chatMembers.forEach(chatMember => {
                if (chatMember.user.userId + '' !== decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]) {
                    setPrivateChatAvatarUrl(chatMember.user.avatarUrl);
                    setPrivateChatName(chatMember.user.username);
                    setPrivateChatUserInfo(chatMember.user);
                    IsUserBlocked(chatMember.user.userId);
                    AmIBlocked(chatMember.user.userId);
                }
            });
        }
    }, [chat]);

    useEffect(scrollToBottom, [messages]);

    useEffect(() => {
        if (files.length > 0 && repliableMessage) {
            setAmountOfVh(60);
        }
        else if (files.length > 0) {
            setAmountOfVh(70);
        }
        else if (repliableMessage) {
            setAmountOfVh(75);
        }
        else {
            setAmountOfVh(85);
        }
    }, [files, repliableMessage]);

    return (
        <div style={{ minHeight: '100vh' }}>
            <div style={{ display: 'flex', height: '10vh', background: 'white' }}>
                <div style={{ display: 'flex', cursor: 'pointer', width: '85%', margin: 'auto 0' }}>
                    {
                        chat.chatType === 'public'
                            ? <img className={styleChat.chat_avatar} src={`http://45.144.222.67:5266/api/files/${chat.avatarUrl}`} alt='img' />
                            : <img className={styleChat.chat_avatar} src={`http://45.144.222.67:5266/api/files/${privateChatAvatarUrl}`} alt='img' />
                    }
                    {
                        chat.chatType === 'public'
                            ? <p>{chat.groupName}</p>
                            : <p>{privateChatName}</p>
                    }
                </div>
                {
                    chat.chatType === 'public'
                        ? <div style={{ display: 'flex', width: '15%' }}>
                            {isJoined
                                ? <button
                                    className={styles.button}
                                    style={{ border: 'none' }}
                                    onClick={() => LeaveChat()}>
                                    <img
                                        style={{ maxHeight: '50px' }}
                                        src={leaveChatImg}
                                        alt='' />
                                </button>
                                : <div></div>}
                        </div>
                        : !isUserBlocked
                            ? <div
                                style={{ display: 'flex', width: '15%' }}>
                                <button
                                    className={styles.button}
                                    style={{ border: 'none' }}
                                    onClick={() => { BlockUser(); }}>
                                    <img
                                        style={{ maxHeight: '30px' }}
                                        src={blockUserImg}
                                        alt='' />
                                </button>
                            </div>
                            : <div
                                style={{ display: 'flex', width: '15%' }}>
                                <button
                                    className={styles.button}
                                    style={{ border: 'none' }}
                                    onClick={() => { UnblockUser(); }}>
                                    <img
                                        style={{ maxHeight: '30px' }}
                                        src={unblockUserImg} alt='' />
                                </button>
                            </div>
                }
            </div>
            <div style={{ height: amountOfVh + 'vh', overflowY: 'auto' }}>
                {
                    messages.map(message => {
                        return <Message key={message.messageId} message={message} setSelectedUser={setSelectedUser} setRepliableMessage={setRepliableMessage} />
                    })
                }
                <div ref={messagesEndRef} />
            </div>
            {
                repliableMessage !== null
                    ? <div style={{ display: 'flex', height: '10vh', backgroundColor: 'white' }}>
                        <div style={{ margin: 'auto 0' }}>
                            <p style={{ margin: '3px 1px', fontSize: '15px' }}>{repliableMessage.sender.firstName} {repliableMessage.sender.lastName}</p>
                            <p style={{ margin: '3px 1px', fontSize: '15px' }}>{repliableMessage.content ? repliableMessage.content : repliableMessage.attachments.length + ' вложений'}</p>
                        </div>
                        <button onClick={() => { setRepliableMessage(null); }} style={{ background: 'none', border: 'none', marginLeft: 'auto', width: '9%', cursor: 'pointer', fontSize: '30px' }}>×</button>
                    </div>
                    : <div></div>
            }
            {
                files.length > 0
                    ? <div style={{ display: 'flex', height: '15vh', backgroundColor: 'white' }}>
                        {
                            Array.from(files).map(file => {
                                return <FileAttachment key={file.name} setFiles={setFiles} file={file} />
                            })
                        }
                    </div>
                    : <div>

                    </div>
            }
            {
                isEmojiListOpen ? <EmojiList setCurrentMessage={setCurrentMessage} currentMessage={currentMessage} currentMessageInput={currentMessageInput}></EmojiList> : <div></div>
            }
            <div style={{ height: '5vh', background: 'white', margin: 'auto', borderColor: 'rgba(178, 178, 178, 0.5)', borderWidth: '1px' }}>
                {
                    isJoined
                        ? amIBlocked
                            ? <button className={styles.button}>Вас заблокировали</button>
                            : <div style={{ display: 'flex', width: '100%', height: '100%' }}>
                                <div style={{ height: '111%', width: '5%' }}>
                                    <label htmlFor="file" className={styles.file_label}></label>
                                    <input accept='image/png, image/gif, image/jpeg' type="file" id="file" className={styles.file_input} multiple onChange={e => { setFiles(e.target.files); }} />
                                </div>
                                <input
                                    ref={currentMessageInput}
                                    className={styles.input}
                                    type="text"
                                    onChange={(e) => setCurrentMessage(e.target.value)}
                                    onKeyDown={(e) => { sendMessage(e, chat.chatId, currentMessage, repliableMessage?.messageId); }}
                                    placeholder="Напишите сообщение..." onPaste={(e) => { onPasteImage(e); }} />
                                <button style={{ height: '111%', width: '10%' }} className={styles.button} onClick={() => { setIsEmojiListOpen(!isEmojiListOpen) }}>😊</button>
                            </div>
                        : <button className={styles.button} onClick={() => { JoinChat() }}>Вступить в чат</button>}
            </div>
        </div>
    )
}

export default ChatMessages;