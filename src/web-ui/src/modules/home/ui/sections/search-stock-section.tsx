"use client";

import { SearchIcon } from "lucide-react";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import { TransactionModal } from "@/modules/transactions/ui/components/transaction-modal";
import { useTransactionModal } from "@/modules/transactions/hooks/use-transaction-modal";

import { Input } from "@/components/ui/input";
import { Form, FormControl, FormField, FormItem } from "@/components/ui/form";
import { Button } from "@/components/ui/button";

const formSchema = z.object({
  searchTerm: z
    .string()
    .min(1, "Your search must contain at least 1 character(s)"),
});

export const SearchStockSection = () => {
  const { onOpen } = useTransactionModal();

  const form = useForm<z.infer<typeof formSchema>>({
    resolver: zodResolver(formSchema),
    values: {
      searchTerm: "",
    },
  });

  const onSubmit = (values: z.infer<typeof formSchema>) => {
    onOpen(values.searchTerm);
  };

  return (
    <>
      <TransactionModal />

      <div className="px-4 lg:px-12 py-8 border-b flex flex-col gap-4 w-full ">
        <div className="flex items-center gap-2 w-full">
          <Form {...form}>
            <form
              onSubmit={form.handleSubmit(onSubmit)}
              className="relative w-full"
              aria-labelledby="search-form"
            >
              <h2 id="search-form" className="sr-only">
                Search for stocks to buy
              </h2>

              <FormField
                name="searchTerm"
                render={({ field }) => (
                  <FormItem>
                    <FormControl>
                      <div className="relative w-full">
                        <Button
                          type="submit"
                          variant="ghost"
                          className="absolute top-1/2 left-3 -translate-y-1/2 size-8 text-neutral-500 p-4 rounded-full shrink-0"
                          aria-label="Search for stocks"
                        >
                          <SearchIcon className="shrink-0" />
                        </Button>
                        <Input
                          {...field}
                          className="pl-14"
                          placeholder="Search stocks to buy..."
                          aria-label="Search stocks"
                        />
                      </div>
                    </FormControl>
                  </FormItem>
                )}
              />
            </form>
          </Form>
        </div>
      </div>
    </>
  );
};
