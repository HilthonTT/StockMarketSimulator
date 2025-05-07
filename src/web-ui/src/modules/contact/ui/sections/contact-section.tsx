"use client";

import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";

import { ContactSchema } from "../../schemas";

export const ContactSection = () => {
  const form = useForm<z.infer<typeof ContactSchema>>({
    resolver: zodResolver(ContactSchema),
    defaultValues: {
      message: "",
    },
  });

  const onSubmit = (values: z.infer<typeof ContactSchema>) => {
    console.log({ values });
  };

  return (
    <div className="w-full max-w-xl mx-auto p-8 bg-white dark:bg-black rounded-lg shadow-lg z-[999999999999]">
      <h2 className="text-3xl font-semibold text-gray-900 dark:text-white mb-6">
        Contact Us
      </h2>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          {/* Message Field */}
          <FormField
            name="message"
            render={({ field }) => (
              <FormItem>
                <FormLabel
                  htmlFor="message"
                  className="text-lg text-gray-700 dark:text-gray-300 mb-2"
                >
                  Your message
                </FormLabel>
                <FormControl>
                  <Textarea
                    {...field}
                    required
                    placeholder="Write your message here..."
                    rows={6}
                    id="message"
                  />
                </FormControl>
                <FormMessage className="text-sm text-red-500" />
              </FormItem>
            )}
          />

          {/* Submit Button */}
          <div>
            <Button type="submit" variant="elevated" className="w-full">
              Send Message
            </Button>
          </div>
        </form>
      </Form>
    </div>
  );
};
