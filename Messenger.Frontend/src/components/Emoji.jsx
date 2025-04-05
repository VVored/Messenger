import React from 'react'

const Emoji = (emoji, setCurrentMessage) => {

    return (
        <span onClick={() => { setCurrentMessage(prev => [...prev, emoji]) }}>
            {emoji}
        </span>
    )
}

export default Emoji