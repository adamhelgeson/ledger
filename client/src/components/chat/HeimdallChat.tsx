import { useState, useRef, useEffect } from 'react'
import { X, Send, Loader2 } from 'lucide-react'
import { useDashboardStore } from '@/stores/dashboardStore'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { cn } from '@/lib/utils'
import { sendChatMessage } from '@/services/api'

interface Message {
  id: string
  role: 'user' | 'assistant'
  content: string
}

const GREETING: Message = {
  id: 'greeting',
  role: 'assistant',
  content:
    'I am Heimdall, the All-Seeing. I observe the flow of gold across the Nine Realms. Ask me about your treasury — spending, balances, or the patterns hidden in your transactions.',
}

export function HeimdallChat() {
  const chatOpen = useDashboardStore((s) => s.chatOpen)
  const setChatOpen = useDashboardStore((s) => s.setChatOpen)
  const [messages, setMessages] = useState<Message[]>([GREETING])
  const [input, setInput] = useState('')
  const [loading, setLoading] = useState(false)
  const bottomRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (chatOpen) {
      setTimeout(() => inputRef.current?.focus(), 300)
    }
  }, [chatOpen])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const handleSend = async () => {
    const text = input.trim()
    if (!text || loading) return

    // Capture history before adding the new user message
    const history = messages.map((m) => ({ role: m.role, content: m.content }))

    const userMsg: Message = { id: crypto.randomUUID(), role: 'user', content: text }
    setMessages((prev) => [...prev, userMsg])
    setInput('')
    setLoading(true)

    try {
      const reply = await sendChatMessage(text, history)
      const assistantMsg: Message = {
        id: crypto.randomUUID(),
        role: 'assistant',
        content: reply,
      }
      setMessages((prev) => [...prev, assistantMsg])
    } catch {
      setMessages((prev) => [
        ...prev,
        {
          id: crypto.randomUUID(),
          role: 'assistant',
          content: 'The Bifrost connection falters. The gods are unavailable at this time.',
        },
      ])
    } finally {
      setLoading(false)
    }
  }

  return (
    <>
      {/* Backdrop */}
      <div
        className={cn(
          'fixed inset-0 z-40 bg-black/40 backdrop-blur-sm transition-opacity duration-300',
          chatOpen ? 'opacity-100 pointer-events-auto' : 'opacity-0 pointer-events-none',
        )}
        onClick={() => setChatOpen(false)}
      />

      {/* Panel */}
      <aside
        className={cn(
          'fixed top-0 right-0 z-50 h-full w-full max-w-sm',
          'bg-bg-card border-l border-border',
          'flex flex-col',
          'transition-transform duration-300 ease-in-out',
          chatOpen ? 'translate-x-0' : 'translate-x-full',
        )}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-border shrink-0">
          <div>
            <h2 className="font-display text-sm font-bold text-gold tracking-wide">Heimdall</h2>
            <p className="text-[10px] font-mono text-text-muted uppercase tracking-widest">
              All-Seeing Oracle
            </p>
          </div>
          <Button variant="ghost" size="icon" onClick={() => setChatOpen(false)}>
            <X size={16} />
          </Button>
        </div>

        {/* Messages */}
        <div className="flex-1 overflow-y-auto px-4 py-4 space-y-4 scrollbar-thin">
          {messages.map((msg) => (
            <div
              key={msg.id}
              className={cn(
                'max-w-[85%] rounded-lg px-3.5 py-2.5 text-sm leading-relaxed animate-fade-in',
                msg.role === 'user'
                  ? 'ml-auto bg-gold/10 border border-gold/20 text-text-primary font-mono text-xs'
                  : 'mr-auto bg-bg-primary border border-border text-text-secondary',
              )}
            >
              {msg.content}
            </div>
          ))}

          {loading && (
            <div className="mr-auto bg-bg-primary border border-border rounded-lg px-3.5 py-2.5 animate-fade-in">
              <Loader2 size={14} className="animate-spin text-gold" />
            </div>
          )}

          <div ref={bottomRef} />
        </div>

        {/* Input */}
        <div className="px-4 py-4 border-t border-border shrink-0">
          <form
            onSubmit={(e) => {
              e.preventDefault()
              handleSend()
            }}
            className="flex gap-2"
          >
            <Input
              ref={inputRef}
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Ask the oracle..."
              className="flex-1 font-mono text-xs"
              disabled={loading}
            />
            <Button type="submit" size="icon" disabled={loading || !input.trim()}>
              <Send size={14} />
            </Button>
          </form>
          <p className="text-[10px] font-mono text-text-muted mt-2 text-center">
            Powered by Yggdrasil AI
          </p>
        </div>
      </aside>
    </>
  )
}
