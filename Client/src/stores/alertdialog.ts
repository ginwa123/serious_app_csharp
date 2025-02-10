import { create, StoreApi, UseBoundStore } from 'zustand';

export interface AlertDialogParams {
  message: string;
  title: string;
}

export interface StoreAlertDialog {
  isOpen: boolean;
  message: string;
  title: string;
  closePromise: Promise<boolean> | null;
  setIsOpen: (isOpen: boolean) => void;
  setMessageAsync: (params: AlertDialogParams) => Promise<boolean>;
  setMessage: (params: AlertDialogParams) => void;
  close: () => void;
}

export const useAlertDialog: UseBoundStore<StoreApi<StoreAlertDialog>> = create((set, get) => ({
  isOpen: false,
  message: '',
  title: '',
  closePromise: null,

  close: () => {
    const { closePromise } = get();
    set({ isOpen: false });
    set({ message: '' });
    set({ title: '' });
    // Resolve the promise when the dialog is closed
    if (closePromise) {
      // @ts-ignore
      closePromise(true); // Resolves the promise
    }
    set({ closePromise: null }); // Reset closePromise
  },

  setIsOpen: (isOpen: boolean) => {
    set({ isOpen });
  },

  setMessageAsync: async (params: AlertDialogParams) => {
    // Set the dialog state
    set({
      isOpen: true,
      message: params.message,
      title: params.title,
    });

    // Create a new promise for the dialog close
    const closePromise = new Promise<boolean>((resolve) => {
      // @ts-ignore
      set({ closePromise: resolve });
    });

    // Return the promise to wait for it to be resolved
    return closePromise;
  },

  setMessage: (params: AlertDialogParams) => {
    set({ title: params.title, message: params.message, isOpen: true });
  },
}));
