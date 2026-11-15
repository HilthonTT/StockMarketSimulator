"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { LoaderIcon, RotateCcwIcon } from "lucide-react";
import { toast } from "sonner";

import { useTRPC } from "@/trpc/client";
import { formatCurrency } from "@/lib/utils";
import { STARTING_CASH } from "@/constants";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

/** Danger zone: start over with fresh virtual cash. */
export const ResetAccountCard = () => {
  const trpc = useTRPC();
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);

  const reset = useMutation(
    trpc.budgets.reset.mutationOptions({
      onSuccess: async () => {
        setOpen(false);
        toast.success("Account reset", {
          description: `You're back to ${formatCurrency(STARTING_CASH)} in cash. Good luck!`,
        });
        // Cash, positions, orders, history and the leaderboard all changed.
        await queryClient.invalidateQueries();
      },
      onError: (error) => {
        toast.error("Couldn't reset your account", { description: error.message });
      },
    })
  );

  return (
    <Card className="border-loss/40">
      <CardHeader>
        <CardTitle className="text-loss">Danger zone</CardTitle>
        <CardDescription>
          Reset your account to start over with {formatCurrency(STARTING_CASH)}. Your positions and trade history are
          deleted and open orders are cancelled. This can&apos;t be undone.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <AlertDialog open={open} onOpenChange={setOpen}>
          <AlertDialogTrigger asChild>
            <Button variant="outline" className="border-loss/50 text-loss hover:bg-loss/10 hover:text-loss">
              <RotateCcwIcon /> Reset account
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Reset your account?</AlertDialogTitle>
              <AlertDialogDescription>
                Everything you own is removed, your trade history is deleted, open orders are cancelled, and your cash
                goes back to {formatCurrency(STARTING_CASH)}. Your leaderboard rank starts over too.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel disabled={reset.isPending}>Keep my account</AlertDialogCancel>
              <AlertDialogAction
                className="bg-loss text-white hover:bg-loss/90"
                disabled={reset.isPending}
                onClick={(event) => {
                  // Keep the dialog open until the request finishes.
                  event.preventDefault();
                  reset.mutate();
                }}
              >
                {reset.isPending && <LoaderIcon className="animate-spin" />}
                Yes, reset everything
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </CardContent>
    </Card>
  );
};
