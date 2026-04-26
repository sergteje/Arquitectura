import { Loader2 } from 'lucide-react'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'

interface DeleteDialogProps {
  isOpen: boolean
  isDark: boolean
  isPending: boolean
  title?: string
  description?: string
  onOpenChange: (open: boolean) => void
  onConfirm: () => void
  confirmLabel?: string
  cancelLabel?: string
}

export function DeleteDialog({ 
  isOpen, 
  isDark, 
  isPending,
  title = 'Are you sure?',
  description = 'This action cannot be undone.',
  onOpenChange, 
  onConfirm,
  confirmLabel = 'Delete',
  cancelLabel = 'Cancel'
}: DeleteDialogProps) {
  return (
    <AlertDialog open={isOpen} onOpenChange={onOpenChange}>
      <AlertDialogContent className={isDark ? 'bg-slate-900 border-slate-700' : 'bg-white'}>
        <AlertDialogHeader>
          <AlertDialogTitle className={isDark ? 'text-white' : 'text-slate-950'}>
            {title}
          </AlertDialogTitle>
          <AlertDialogDescription className={isDark ? 'text-slate-400' : 'text-slate-700'}>
            {description}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel className={isDark ? 'bg-slate-800 text-white hover:bg-slate-700' : ''}>
            {cancelLabel}
          </AlertDialogCancel>
          <AlertDialogAction 
            onClick={onConfirm} 
            className="bg-red-600 hover:bg-red-700 text-white"
          >
            {isPending ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
            {confirmLabel}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}