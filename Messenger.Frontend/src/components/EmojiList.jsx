import React from 'react'

const emojiList = ["👍", "❤️", "🔥", "✨", "🚀", "😂", "😍", "🤔", "😊", "🎉", "🐶", "🐱", "🦁", "🐯", "🦄", "🍎", "🍕", "🍔", "☕", "🍩", "⚽", "🎮", "🎲", "🎸", "🎯", "🌍", "🌞", "🌈", "🌊", "❄️", "📱", "💻", "🎧", "📚", "✏️", "❤️‍🔥", "💯", "👑", "🎁", "💎", "🤖", "👽", "👻", "💀", "👾", "🛒", "💰", "💵", "🛍️", "🎊", "⚠️", "🚨", "✅", "❌", "🔄"]

const EmojiList = ({ setCurrentMessage, currentMessage, currentMessageInput }) => {

    return (
        <div style={{width: '300px', position: 'fixed', top: '75%', backgroundColor: 'white', padding: '5px', borderRadius: '5px', right: '25px'}}>
            {emojiList.map((emoji, index) => (
                <span style={{cursor: 'pointer', userSelect: 'none'}} key={index + 1} onClick={() => { setCurrentMessage(currentMessage + emoji); currentMessageInput.current.value = currentMessage + emoji }}>
                    {emoji}
                </span>
            ))}
        </div>
    )
}

export default EmojiList;