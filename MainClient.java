package com.company;

import java.io.*;
import java.net.*;
import java.util.*;

public class Main {

    private static BufferedReader reader;
    private static BufferedReader in;
    private static BufferedWriter out;

    public static void main(String[] args) throws IOException {
        for (int k = 2000; k <= 2046; k++) {
            try (Socket socket = new Socket()) {
                InetSocketAddress addr = new InetSocketAddress("127.0.0.1", k);
                socket.connect(addr, 25);
                System.out.println("socket = " + socket);

                reader = new BufferedReader(new InputStreamReader(System.in));
                in = new BufferedReader(new InputStreamReader(socket.getInputStream()));
                out = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream()));

                System.out.println("Enter text('exit' for exit):");
                //out.write("Do you understand me?");
                Boolean a = true;
                while (a = true) {
                    String word = reader.readLine();
                    out.write(word + "\n");
                    out.flush();

                    String serverWord = in.readLine();
                    System.out.println("Server: " + serverWord);

                    if(serverWord.equals("Yes, i do")){
                        System.out.println("Neighbor: 127.0.0.1:" + k);
                    }
                    if(word.equals("exit")){
                        a = false;
                        socket.close();
                    }
                }
            }
            finally {
            System.out.println("closing...");

            }
        }
    }
}


