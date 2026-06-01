import { useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore';
import { useNotificationStore } from '@/stores/notificationStore';
import type { Notification } from '@/types';

export function useSignalR() {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const token = useAuthStore((s) => s.token);
  const addNotification = useNotificationStore((s) => s.addNotification);

  const connect = useCallback(async () => {
    if (!token || connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    if (connectionRef.current) {
      await connectionRef.current.stop();
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('ReceiveNotification', (notification: Notification) => {
      addNotification(notification);
    });

    connection.on('ReservationStatusChanged', (data: { reservationId: string; status: string }) => {
      addNotification({
        id: crypto.randomUUID(),
        userId: '',
        title: 'Estado de reservación actualizado',
        message: `La reservación ${data.reservationId} cambió a ${data.status}`,
        type: 'ReservationStatus',
        isRead: false,
        referenceId: data.reservationId,
        createdAt: new Date().toISOString(),
      });
    });

    connection.on('NewAnnouncement', (announcement) => {
      addNotification({
        id: crypto.randomUUID(),
        userId: '',
        title: announcement.title,
        message: announcement.body,
        type: 'Announcement',
        isRead: false,
        referenceId: announcement.id,
        createdAt: new Date().toISOString(),
      });
    });

    connection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
    });

    connection.onreconnected(() => {
      console.log('SignalR reconnected');
    });

    connection.onclose(() => {
      console.log('SignalR connection closed');
    });

    try {
      await connection.start();
      connectionRef.current = connection;
    } catch (err) {
      console.error('SignalR connection error:', err);
    }
  }, [token, addNotification]);

  useEffect(() => {
    connect();
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }
    };
  }, [connect]);

  return connectionRef;
}
