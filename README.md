# SIP_project

USER MANUAL

1) The very first task you will perform will be connecting to a VoIP PBX system. This task is called registration, where authentication id, username, password and domain name are to be entered correctly to be register SIP client to the VoIP PBX.
2) Then create your own certification using CA (certificate authority) system like SimpleCA, then install the certificate and place the certificate in Trusted Root Certification Authorities folder which will enable TLS encryption within your softphone when you run the SIP_encryption from the source folder.
3) Next step is to enable RTP encryption. The RTP encryption can be used in the case of a softphone application by setting its phone line object's SRTPMode property with the preferred SRTP mode.
4) Then run the Call_Make_Accept to establish a secure session. During a voice call parties can communicate by sending and receiving voice stream data through the media devices, for example: microphone, speaker, which means that the voice data from a party's microphone and it arrives to the other party's speaker. If the client is registered to a third party, the call will take place and if the client is from a blacklist then call gets rejected by the firewall itself.

Additional Features:

5) You can run codecs_handler and noise reduction one by one. Codecs_handler lets you select the specific audio or video codec to make the call.
6) The attacker can simply spoil the quality of the call by injecting noise packets in the communication stream. So if you run the noise reduction from the source it removes the noise from the signal.
