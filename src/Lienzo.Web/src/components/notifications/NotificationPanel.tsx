import { useNotificationStore } from '@/stores/notificationStore';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { ScrollArea } from '@/components/ui/ScrollArea';
import { X, Bell, CheckCheck, Info, AlertTriangle, Calendar } from 'lucide-react';
import { cn } from '@/lib/utils';
import { formatDateTime } from '@/lib/utils';

const typeIcons: Record<string, React.ReactNode> = {
  ReservationStatus: <Calendar className="h-4 w-4 text-blue-600" />,
  Announcement: <Bell className="h-4 w-4 text-accent-500" />,
  Cancellation: <AlertTriangle className="h-4 w-4 text-red-600" />,
};

export function NotificationPanel() {
  const { isOpen, closePanel, notifications, unreadCount, markAsRead, markAllAsRead } =
    useNotificationStore();

  const handleMarkAsRead = async (id: string) => {
    try {
      await api.patch(`/notifications/${id}/read`);
      markAsRead(id);
    } catch {
      // offline
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      await api.patch('/notifications/read-all');
      markAllAsRead();
    } catch {
      // offline
    }
  };

  if (!isOpen) return null;

  return (
    <>
      <div className="fixed inset-0 z-50 bg-black/30" onClick={closePanel} />
      <div className="fixed right-0 top-0 bottom-16 lg:bottom-0 z-50 w-full max-w-sm bg-white shadow-xl border-l border-primary-100 animate-slide-in-right">
        <div className="flex items-center justify-between px-4 py-3 border-b border-primary-100">
          <div className="flex items-center gap-2">
            <Bell className="h-5 w-5 text-primary-600" />
            <h3 className="font-heading font-semibold text-primary-800">Notificaciones</h3>
            {unreadCount > 0 && (
              <span className="inline-flex items-center justify-center h-5 px-1.5 rounded-full bg-red-100 text-red-700 text-xs font-medium">
                {unreadCount}
              </span>
            )}
          </div>
          <div className="flex items-center gap-1">
            {unreadCount > 0 && (
              <Button variant="ghost" size="sm" onClick={handleMarkAllAsRead}>
                <CheckCheck className="h-4 w-4 mr-1" />
                Leer todo
              </Button>
            )}
            <button onClick={closePanel} className="p-1.5 rounded-lg hover:bg-primary-50">
              <X className="h-5 w-5 text-primary-400" />
            </button>
          </div>
        </div>

        <ScrollArea className="h-full pb-4">
          {notifications.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 px-4 text-center">
              <Bell className="h-12 w-12 text-primary-200 mb-3" />
              <p className="text-primary-500 font-medium">Sin notificaciones</p>
              <p className="text-primary-400 text-sm">No tienes notificaciones nuevas</p>
            </div>
          ) : (
            <div className="divide-y divide-primary-50">
              {notifications.map((n) => (
                <button
                  key={n.id}
                  onClick={() => !n.isRead && handleMarkAsRead(n.id)}
                  className={cn(
                    'w-full text-left px-4 py-3 transition-colors hover:bg-primary-50',
                    !n.isRead && 'bg-accent-50/30'
                  )}
                >
                  <div className="flex items-start gap-3">
                    <div className="mt-0.5 flex-shrink-0">
                      {typeIcons[n.type] || <Info className="h-4 w-4 text-primary-400" />}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className={cn('text-sm', !n.isRead ? 'font-semibold text-primary-800' : 'text-primary-600')}>
                        {n.title}
                      </p>
                      <p className="text-xs text-primary-400 mt-0.5 line-clamp-2">{n.message}</p>
                      <p className="text-[10px] text-primary-300 mt-1">
                        {formatDateTime(n.createdAt)}
                      </p>
                    </div>
                    {!n.isRead && (
                      <span className="h-2 w-2 rounded-full bg-accent-500 flex-shrink-0 mt-1.5" />
                    )}
                  </div>
                </button>
              ))}
            </div>
          )}
        </ScrollArea>
      </div>
    </>
  );
}
